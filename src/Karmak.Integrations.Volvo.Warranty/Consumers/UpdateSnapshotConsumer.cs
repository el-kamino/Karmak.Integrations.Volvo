using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Consumers
{
    public class UpdateSnapshotConsumer : IConsumer<UpdateSnapshotMessage>
    {
        private readonly ILogger<UpdateSnapshotConsumer> _logger;
        private readonly IClaimsService _claimsService;
        private readonly IFusionClient _fusionClient;

        public UpdateSnapshotConsumer(ILogger<UpdateSnapshotConsumer> logger, IClaimsService claimsService, IFusionClient fusionClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _claimsService = claimsService ?? throw new ArgumentNullException(nameof(claimsService));
            _fusionClient = fusionClient ?? throw new ArgumentNullException(nameof(fusionClient));
        }

        public async Task Consume(ConsumeContext<UpdateSnapshotMessage> context)
        {
            TrackReceivedForProcessing(context);

            var update = context.Message.UpdateSnapshot;
            var claim = await _claimsService.AddToUpdateSnapshotHistory(context.Message.ClaimId, update, FusionIdentity.VolvoStatus);

            if (update.Type == UpdateType.Reconciliation)
            {
                if (PaymentAlreadyProcessed(update, claim))
                {
                    TrackReconciliationAlreadyProcessed(update, claim);
                    return;
                }
                else
                {
                    await ConsumeReconciliationUpdate(update, claim);
                }
            }

            TrackProcessingResults(update, claim);
        }

        private async Task ConsumeReconciliationUpdate(UpdateSnapshot update, Claim claim)
        {
            var paymentInfo = ExtractPaymentInformation(update);
            await _fusionClient.ProcessWarrantyPaymentAsync(paymentInfo, CancellationToken.None);

            SetProcessedByFusionDateTime(update, claim);
            await _claimsService.Update(claim, claim.Id, FusionIdentity.VolvoRecon);
        }

        private bool PaymentAlreadyProcessed(UpdateSnapshot update, Claim claim)
        {
            return claim.UpdateSnapshots.Any(claimUpdate =>
            {
                return claimUpdate.Type == UpdateType.Reconciliation
                    && claimUpdate.ProcessedByFusionDateTime != null
                    && claimUpdate.IsDuplicate(update);
            });
        }

        private void SetProcessedByFusionDateTime(UpdateSnapshot update, Claim claim)
        {
            claim.UpdateSnapshots = claim.UpdateSnapshots.Select(claimUpdate =>
            {
                if (claimUpdate.IsDuplicate(update))
                {
                    claimUpdate.ProcessedByFusionDateTime = DateTime.UtcNow;
                }
                return claimUpdate;
            }).ToList();
        }

        private static WarrantyPaymentInformation ExtractPaymentInformation(UpdateSnapshot update) => new WarrantyPaymentInformation
        {
            Id = update.Id,
            ClaimNumber = update.RepairOrderNumber,
            RepairOrderNumber = update.RepairOrderNumber,
            DealerCode = update.DealerCode,
            ToBePaidAmount = update.ApprovedAmount.Value,
            ProcessDate = update.ProcessDate
        };

        private void TrackReceivedForProcessing(ConsumeContext<UpdateSnapshotMessage> context)
        {
            _logger.LogInformationWithMetadata($"Received {nameof(UpdateSnapshotMessage)} for processing.", new Dictionary<string, string> {
                { "Claim.Id", context.Message.ClaimId },
                { "UpdateSnapshotMessage.TransactionId", context.Message.TransactionId },
                { "UpdateSnapshot.Id", context.Message.UpdateSnapshot.Id },
                { "Redelivery.Attempt", context.GetRedeliveryCount().ToString() },
                { "RepairOrderNumber", context.Message.UpdateSnapshot.RepairOrderNumber }
            });
        }

        private void TrackReconciliationAlreadyProcessed(UpdateSnapshot update, Claim claim)
        {
            _logger.LogInformationWithMetadata($"Processing failed. Reconciliation push update has already been processed.", new Dictionary<string, string> {
                { "Claim.Id", claim.Id },
                { "Claim.Identifier", claim.Identifier },
                { "Claim.CorrelationId", claim.CorrelationId },
                { "UpdateSnapshot.Id", update.Id },
                { "RepairOrderNumber", update.RepairOrderNumber },
                { TelemetryKeys.DealerCode, update.DealerCode },
            });
        }

        private void TrackProcessingResults(UpdateSnapshot update, Claim claim)
        {
            var businessProcess = update.Type == UpdateType.Reconciliation
                ? "Reconciliation Processed"
                : "Status Update Processed";

            _logger.LogInformationWithMetadata($"Finished processing claim's {update.Type} update.", new Dictionary<string, string> {
                { "Claim.Id", claim.Id },
                { "Claim.Identifier", claim.Identifier },
                { "Claim.CorrelationId", claim.CorrelationId },
                { "RepairOrderNumber", update.RepairOrderNumber },
                { "UpdateSnapshot.Id", update.Id },
                { TelemetryKeys.DealerCode, update.DealerCode },
                { TelemetryKeys.KarmakAccountNumber, claim?.Dealer?.Account?.Code},
                { TelemetryKeys.OEM, TelemetryValues.Volvo },
                { TelemetryKeys.BusinessProcess, businessProcess },
                { TelemetryKeys.Module, TelemetryValues.Warranty }});
        }
    }
}
