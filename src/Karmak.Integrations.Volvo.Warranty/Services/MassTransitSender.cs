using System;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using MassTransit;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public sealed class MassTransitSender : IInboundMessageSender
    {
        private readonly IBusControl _bus;

        public MassTransitSender(IBusControl bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }
        public Task PublishAsync<T>(T message) where T : class
        {
            return _bus.Publish(message);
        }
    }
}
