using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IInboundMessageSender
    {
        Task PublishAsync<T>(T message) where T : class;
    }
}
