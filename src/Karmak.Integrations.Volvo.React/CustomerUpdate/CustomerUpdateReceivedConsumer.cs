using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate
{
    internal class CustomerUpdateReceivedConsumer : IConsumer<CustomerUpdateReceived>
    {
        private readonly IProcessCustomerUpdate _processor;
        private readonly IKarmakBlobClient _claimCheckClient;

        public CustomerUpdateReceivedConsumer(
            IProcessCustomerUpdate processor,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient)
        {
            _processor = processor;
            _claimCheckClient = claimCheckClient;
        }

        public async Task Consume(ConsumeContext<CustomerUpdateReceived> context)
        {
            var update = await _claimCheckClient.RetrieveAsync<Contracts.CustomerUpdates.Data.CustomerUpdate>(context.Message.BlobName);

            string transmissionType = update.ForceTransmission
                ? TransmissionType.Historical
                : TransmissionType.New;

            await _processor.Execute(update, transmissionType);
        }
    }
}
