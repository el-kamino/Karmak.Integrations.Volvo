using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.UsedVehicleSales.V5_14_4
{
    internal class RetransmitVehicleSaleConsumer : IConsumer<RetransmitVehicleSales>
    {
        private readonly IProcessUsedVehicleSales _processor;
        private readonly IKarmakBlobClient _claimCheckClient;

        public RetransmitVehicleSaleConsumer(
            IProcessUsedVehicleSales processor,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _processor = processor;
            _claimCheckClient = claimCheckClient;
        }

        public async Task Consume(ConsumeContext<RetransmitVehicleSales> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<VehicleSalesOrder>(context.Message.BlobName);
            await _processor.Execute(update, TransmissionType.Historical);
        }
    }
}
