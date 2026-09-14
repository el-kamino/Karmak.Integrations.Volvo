using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public sealed class ProcessClaimReconciliationHandler : IShowServiceProcessingAdvisoryHandler
    {
        private readonly IInboundMessageSender _sender;
        private readonly IReconciliationToUpdateSnapshotMapper _mapper;
        private readonly IPushValidationHandler _validationHandler;
        private readonly IUpdateSnapshotCorrelationHandler _correlationHandler;
        private readonly ILogger<ProcessClaimReconciliationHandler> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;

        public ProcessClaimReconciliationHandler(IInboundMessageSender sender,
            IReconciliationToUpdateSnapshotMapper mapper,
            IPushValidationHandler validationHandler,
            ILogger<ProcessClaimReconciliationHandler> logger,
            IExtendedLoggingClient extendedLoggingClient,
            IUpdateSnapshotCorrelationHandler correlationHandler)
        {
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _validationHandler = validationHandler ?? throw new ArgumentNullException(nameof(validationHandler));
            _correlationHandler = correlationHandler ?? throw new ArgumentNullException(nameof(correlationHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
        }

        public async Task<Result> HandleAsync(ShowServiceProcessingAdvisoryType request)
        {
            try
            {
                var transactionId = Guid.NewGuid().ToString();
                var updateSnapshotMessages = await ExtractUpdateSnapshotMessages(request, transactionId);

                _logger.LogInformationWithMetadata("Mapped reconciliation updates from Push response.",
                    new Dictionary<string, string> {
                        {"Update Count", updateSnapshotMessages.Count().ToString() },
                        {"UpdateSnapshotMessage.TransactionId", transactionId}
                    });

                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(updateSnapshotMessages), "Mapped Reconciliation Updates", VolvoLogContentType.JSON);

                var validationUpdateMessagePairs = _validationHandler.Validate(updateSnapshotMessages);

                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(validationUpdateMessagePairs), "Validated Reconciliation Updates", VolvoLogContentType.JSON);

                await TrackInvalidUpdates(validationUpdateMessagePairs.Where(x => x?.Errors != null), validationUpdateMessagePairs.Count());

                var validUpdates = validationUpdateMessagePairs.Where(x => x.Errors == null);
                await TrackValidUpdates(validUpdates);

                var random = new Random();
                foreach (var update in validUpdates)
                {
                    if (IsMoreThanOneUpdateForClaim(update, validUpdates))
                    {
                        await _sender.PublishAsync(update.UpdateSnapshotMessage);
                        await Task.Delay(random.Next(3000, 4000));
                    }
                    else
                    {
                        await _sender.PublishAsync(update.UpdateSnapshotMessage);
                    }
                }
                TrackSentUpdates(validUpdates, validationUpdateMessagePairs.Count());

                return validUpdates.Count() != updateSnapshotMessages.Count() ? Result.Fault("Message contained invalid reconciliation updates.") : Result.Success();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return Result.Fault("Error occurred, reconciliation update processing failed.");
            }
        }

        private bool IsMoreThanOneUpdateForClaim(ValidatedUpdateSnapshotMessage update, IEnumerable<ValidatedUpdateSnapshotMessage> updates)
        {
            return updates.Count(x => x.UpdateSnapshotMessage.UpdateSnapshot.RepairOrderNumber == update.UpdateSnapshotMessage.UpdateSnapshot.RepairOrderNumber) > 1;
        }

        private async Task<IEnumerable<UpdateSnapshotMessage>> ExtractUpdateSnapshotMessages(ShowServiceProcessingAdvisoryType request, string transactionId)
        {
            string paCode = CleanPaCode(request.ApplicationArea.Destination.DealerNumberID.Value);

            var updates = new List<UpdateSnapshotMessage>();
            foreach (var x in request.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory)
            {
                foreach (var y in x.ServiceProcessingAdvisoryHeader.DispositionPayments)
                {
                    var createdUpdates = await CreateUpdateSnapshotMessages(y.RepairOrderReconciliation, y.ProcessDate, paCode, transactionId);
                    updates.AddRange(createdUpdates);
                }
            }
            return updates;
        }

        private async Task<IEnumerable<UpdateSnapshotMessage>> CreateUpdateSnapshotMessages(RepairOrderReconciliationType[] reconciliations, DateTime processDate, string paCode, string transactionId)
        {
            var updateSnapshotMessages = new List<UpdateSnapshotMessage>();
            foreach (var reconciliation in reconciliations)
            {
                var updates = reconciliation.JobReconciliation.Select(jobReconciliation =>
                {
                    var reconciliationParent = reconciliation;
                    reconciliationParent.JobReconciliation = new JobReconciliationExtended[] { jobReconciliation };
                    return reconciliationParent;
                });

                var claims = await _correlationHandler.Correlate(reconciliation.DocumentID.Value);

                var createdMessages = updates.Select(update => CreateUpdateSnapshotMessage(claims, update, processDate, paCode, transactionId));

                updateSnapshotMessages.AddRange(createdMessages);
            }
            return updateSnapshotMessages;
        }

        private UpdateSnapshotMessage CreateUpdateSnapshotMessage(IEnumerable<Claim> claims, RepairOrderReconciliationType update, DateTime processDate, string paCode, string transactionId)
        {
            var claim = GetClaimByJobIdentifier(claims, update.JobReconciliation.First().JobNumberString);

            return new UpdateSnapshotMessage()
            {
                TransactionId = transactionId,
                ClaimId = claim?.Id,
                UpdateSnapshot = MapUpdateSnaphot(update, claim, paCode, processDate)
            };
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

        private UpdateSnapshot MapUpdateSnaphot(RepairOrderReconciliationType reconciliation, Claim claim, string paCode, DateTime processDate)
        {
            try
            {
                return _mapper.Map(reconciliation, processDate, paCode, claim, DateTime.UtcNow);
            }
            catch (Exception)
            {
                _logger.LogInformationWithMetadata("Failed to map reconciliation update.", new Dictionary<string, string> {
                    { "ProcessDate", processDate.ToString() },
                    { "DealerCode", paCode }
                });
                return null;
            }
        }

        private string CleanPaCode(string paCode) =>
            paCode.Replace("!", "");

        private async Task TrackInvalidUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> invalidUpdates, int updateCount)
        {
            if (invalidUpdates.Any())
            {
                var updateInfo = string.Join(", ", invalidUpdates.Select(x => x.UpdateSnapshotMessage.UpdateSnapshot.RepairOrderNumber + "-" + x.UpdateSnapshotMessage.UpdateSnapshot.ProcessDate.ToString("yyyy-MM-dd")));
                _logger.LogInformationWithMetadata("Push response contained invalid reconciliation updates",
                    new Dictionary<string, string> {
                        { "Invalid Update Count", invalidUpdates.Count().ToString() },
                        { "Total Update Count",  updateCount.ToString() },
                        { "PA Code", invalidUpdates.First().UpdateSnapshotMessage.UpdateSnapshot.DealerCode },
                        { "RepairOrderNumber-ProcessDate", updateInfo },
                        { "Failure Date", DateTime.UtcNow.ToString("yyyy-MM-dd") },
                    });
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(invalidUpdates), "Invalid Reconciliation Updates", VolvoLogContentType.JSON);
            }
        }

        private async Task TrackValidUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> validUpdates)
        {
            if (validUpdates.Any())
            {
                await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(validUpdates), "Valid Reconciliation Updates", VolvoLogContentType.JSON);
            }
        }

        private void TrackSentUpdates(IEnumerable<ValidatedUpdateSnapshotMessage> validUpdates, int updateCount)
        {
            if (validUpdates.Any())
            {
                _logger.LogInformationWithMetadata("Valid reconciliation updates sent to consumer.",
                    new Dictionary<string, string> {
                        { "Valid Update Count", validUpdates.Count().ToString() },
                        { "Total Update Count", updateCount.ToString() }
                    });
            }
            else
            {
                _logger.LogInformation("Reconciliation fetch response did not contain any valid updates.");
            }
        }
    }
}
