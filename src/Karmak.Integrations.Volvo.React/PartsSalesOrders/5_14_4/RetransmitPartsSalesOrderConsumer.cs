using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    internal class RetransmitPartsSalesOrderConsumer : IConsumer<RetransmitPartsSalesOrder>
    {
        private readonly IProcessPartsSalesOrder _processor;
        private readonly IKarmakBlobClient _claimCheckClient;

        public RetransmitPartsSalesOrderConsumer(
            IProcessPartsSalesOrder processor,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _processor = processor;
            _claimCheckClient = claimCheckClient;
        }

        public async Task Consume(ConsumeContext<RetransmitPartsSalesOrder> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<PartsSalesOrder>(context.Message.BlobName);
            await _processor.Execute(update, TransmissionType.Historical);
        }
    }
}
