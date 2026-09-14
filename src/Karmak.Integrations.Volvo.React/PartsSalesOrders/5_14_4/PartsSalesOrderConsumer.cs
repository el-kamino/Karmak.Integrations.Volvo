using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    internal class PartsSalesOrderConsumer : IConsumer<PartsSalesOrderReceived>
    {
        private readonly IProcessPartsSalesOrder _processor;
        private readonly IKarmakBlobClient _claimCheckClient;

        public PartsSalesOrderConsumer(
            IProcessPartsSalesOrder processor,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _processor = processor;
            _claimCheckClient = claimCheckClient;
        }

        public async Task Consume(ConsumeContext<PartsSalesOrderReceived> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<PartsSalesOrder>(context.Message.BlobName);

            string transmissionType = update.ForceTransmission ? TransmissionType.Historical : TransmissionType.New;
            await _processor.Execute(update, transmissionType);
        }
    }
}
