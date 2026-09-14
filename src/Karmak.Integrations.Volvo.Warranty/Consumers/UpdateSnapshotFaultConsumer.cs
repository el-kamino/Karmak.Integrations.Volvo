using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Consumers
{
    public class UpdateSnapshotFaultConsumer : IConsumer<Fault<UpdateSnapshotMessage>>
    {
        private readonly ILogger<UpdateSnapshotFaultConsumer> _logger;

        public UpdateSnapshotFaultConsumer(ILogger<UpdateSnapshotFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<UpdateSnapshotMessage>> context)
        {
            var message = context.Message.Message;
            var messageMetadata = new Dictionary<string, string> {
                { "Claim.Id", message.ClaimId },
                { "Claim.RepairOrder.Identifier", message.UpdateSnapshot.RepairOrderNumber },
                { "UpdateSnapshot.Id", message.UpdateSnapshot.Id },
                { "UpdateSnapshot.ProcessDate", message.UpdateSnapshot.ProcessDate.ToString("yyyy-MM-dd")},
                { "UpdateSnapshot.DealerCode", message.UpdateSnapshot.DealerCode },
                { "UpdateSnapshotMessage.TransactionId", message.TransactionId }
            };

            _logger.LogInformationWithMetadata($"Failed to process claim's {message.UpdateSnapshot.Type} push update.", messageMetadata);

            return Task.CompletedTask;
        }
    }
}
