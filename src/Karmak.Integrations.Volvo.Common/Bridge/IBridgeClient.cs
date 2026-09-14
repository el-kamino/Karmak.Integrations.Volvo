using System.Threading;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public interface IBridgeClient {
        Task<TResponse> SubmitMessageAsync<TMessage, TResponse>(string contractName, TMessage message, CancellationToken cancellationToken);
    }
}
