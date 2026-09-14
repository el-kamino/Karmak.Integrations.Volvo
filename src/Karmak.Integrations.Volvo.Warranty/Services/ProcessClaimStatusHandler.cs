using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public sealed class ProcessClaimStatusHandler : IShowServiceProcessingAdvisoryHandler
    {

        private readonly IInboundMessageSender _sender;
        private readonly IStatusToUpdateSnapshotMapper _mapper;
        private readonly IPushValidationHandler _validationHandler;
        private readonly ILogger<ProcessClaimStatusHandler> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly IUpdateSnapshotCorrelationHandler _correlationHandler;
        private int _totalUpdateCount;

        public ProcessClaimStatusHandler(IInboundMessageSender sender, IStatusToUpdateSnapshotMapper mapper, IPushValidationHandler validationHandler, ILogger<ProcessClaimStatusHandler> logger, IExtendedLoggingClient extendedLoggingClient, IUpdateSnapshotCorrelationHandler correlationHandler)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
            _correlationHandler = correlationHandler ?? throw new ArgumentNullException(nameof(correlationHandler));
        }

        public async Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request)
        {
            try
            {
                await _extendedLoggingClient.Execute(ToXml.Document(request).OuterXml, "Premapping Status Updates", VolvoLogContentType.XML);
                var transactionId = Guid.NewGuid().ToString();

                var updateSnapshotMessages = await ExtractUpdateSnapshotMessages(request, transactionId);
                _totalUpdateCount = updateSnapshotMessages.Count();

                _logger.LogInformationWithMetadata("Mapped status updates from Push response",
                    new Dictionary<string, string> {
                        { "Update Count", _totalUpdateCount.ToString() },
                        { "UpdateSnapshotMessage.TransactionId", transactionId}
                    });
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(updateSnapshotMessages), "Mapped Status Updates", VolvoLogContentType.JSON);

                var validationUpdateMessagePairs = _validationHandler.Validate(updateSnapshotMessages.ToArray());
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(validationUpdateMessagePairs), "Validated Status Updates", VolvoLogContentType.JSON);

                await TrackInvalidUpdates(validationUpdateMessagePairs.Where(x => x?.Errors != null));
                var validUpdates = validationUpdateMessagePairs.Where(x => x.Errors == null);
                await TrackValidUpdates(validUpdates);

                await Task.WhenAll(validUpdates.Select((x) => _sender.PublishAsync(x.UpdateSnapshotMessage)));
                TrackSentUpdates(validUpdates);

                return validUpdates.Count() != _totalUpdateCount ? Result.Fault("Message contained invalid status updates.") : Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Result.Fault("Error occurred, status update processing failed.");
            }
        }

        private async Task<IEnumerable<UpdateSnapshotMessage>> ExtractUpdateSnapshotMessages(ShowServiceProcessingAdvisoryType request, string transactionId)
        {
            string paCode = CleanPaCode(request.ApplicationArea.Destination.DealerNumberID.Value);

            var updates = new List<UpdateSnapshotMessage>();
            foreach (var x in request.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory)
            {
                foreach (var y in x.ServiceProcessingAdvisoryHeader.DispositionPayments)
                {
                    var createdUpdates = await CreateUpdateSnapshotMessages(y.RepairOrderReconciliation, x.ServiceProcessingAdvisoryHeader.DocumentDateTime, paCode, transactionId);
                    updates.AddRange(createdUpdates);
                }
            }
            return updates;
        }

        private async Task<IEnumerable<UpdateSnapshotMessage>> CreateUpdateSnapshotMessages(RepairOrderReconciliationType[] reconciliations, DateTime processDate, string paCode, string transactionId)
        {
            var groupedReconciliations = reconciliations.GroupBy(x => x.DocumentID.Value);

            var updateSnapshotMessages = new List<UpdateSnapshotMessage>();
            foreach (var group in groupedReconciliations)
            {
                var updates = group.SelectMany(reconciliation =>
                {
                    return reconciliation.JobReconciliation.Select(jobReconciliation =>
                    {
                        var reconciliationParent = reconciliation;
                        reconciliationParent.JobReconciliation = new JobReconciliationExtended[] { jobReconciliation };
                        return reconciliationParent;
                    });
                });

                var claims = await _correlationHandler.Correlate(group.Key);

                var createdMessages = updates.Select(update => CreateUpdateSnapshotMessage(claims, update, processDate, paCode, transactionId));

                updateSnapshotMessages.AddRange(createdMessages);
            }

            return updateSnapshotMessages;
        }

        private UpdateSnapshotMessage CreateUpdateSnapshotMessage(IEnumerable<Claim> claims, RepairOrderReconciliationType update, DateTime processDate, string paCode, string transactionId)
        {
            return new UpdateSnapshotMessage
            {
                TransactionId = transactionId,
                ClaimId = GetClaimByJobIdentifier(claims, update.JobReconciliation.First().JobNumberString)?.Id,
                UpdateSnapshot = MapUpdateSnapshot(update, paCode, processDate)
            };
        }

        private UpdateSnapshot MapUpdateSnapshot(RepairOrderReconciliationType update, string paCode, DateTime processDate)
        {
            try
            {
                return _mapper.Map(update, processDate, paCode);
            }
            catch (Exception)
            {
                _logger.LogInformationWithMetadata("Failed to map status update.", new Dictionary<string, string> {
                    { "ProcessDate", processDate.ToString() },
                    { "DealerCode", paCode }
                });
                return null;
            }
        }

        private Claim GetClaimByJobIdentifier(IEnumerable<Claim> claims, string jobIdentifier)
        {
            return claims?.Count() switch
            {
                null => null,
                1 => claims.First(),
                _ => claims.FirstOrDefault(c => c.RepairOrder.Jobs.Any(job => job.Identifier.Equals(jobIdentifier)))
            };
        }

        private string CleanPaCode(string paCode) =>
            paCode.Replace("!", "");

        private async Task TrackInvalidUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> invalidUpdates)
        {
            if (invalidUpdates.Any())
            {
                _logger.LogInformationWithMetadata("Push response contained invalid status updates",
                    new Dictionary<string, string> {
                        { "Invalid Update Count", invalidUpdates.Count().ToString() },
                        { "Total Update Count", _totalUpdateCount.ToString() }
                    });
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(invalidUpdates), "Invalid Status Updates", VolvoLogContentType.JSON);
            }
        }

        private async Task TrackValidUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> validUpdates)
        {
            if (validUpdates.Any())
            {
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(validUpdates), "Valid Status Updates", VolvoLogContentType.JSON);
            }
        }

        private void TrackSentUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> validUpdates)
        {
            if (validUpdates.Any())
            {
                _logger.LogInformationWithMetadata("Valid status push updates sent to consumer.",
                    new Dictionary<string, string> {
                        { "Valid Update Count", validUpdates.Count().ToString() },
                        { "Total Update Count", _totalUpdateCount.ToString() }
                    });
            }
        }
    }
}
