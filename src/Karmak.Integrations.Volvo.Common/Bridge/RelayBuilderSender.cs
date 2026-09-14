using Microsoft.Azure.Relay;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public interface IRelayBuilder
    {
        IRelaySender Build(string channelName);
    }

    public class RelayBuilder : IRelayBuilder
    {
        private readonly string _relayNamespace;
        private readonly TokenProvider _tokenProvider;

        public RelayBuilder(string relayNamespace, TokenProvider tokenProvider)
        {
            _relayNamespace = relayNamespace;
            _tokenProvider = tokenProvider;
        }

        public IRelaySender Build(string channelName)
        {
            return new RelaySender(_relayNamespace, channelName, _tokenProvider);
        }
    }

    public interface IRelaySender
    {
        Task<ResponseDetails> SendRequest(RequestDetails request);
    }

    public class RelaySender : IRelaySender
    {
        private readonly HybridConnectionClient _sender;

        public RelaySender(string relayNamespace, string channelName, TokenProvider tokenProvider)
        {
            _sender = new HybridConnectionClient(new Uri(string.Format("sb://{0}/{1}", relayNamespace, channelName)), tokenProvider);
            _sender.UseBuiltInClientWebSocket = true;
        }

        public async Task<ResponseDetails> SendRequest(RequestDetails request)
        {
            ResponseDetails response = new ResponseDetails();
            var relayConnection = await _sender.CreateConnectionAsync();

            var reads = new Func<Task>(async () =>
            {
                var reader = new StreamReader(relayConnection);
                string line = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                {
                    throw new RelayCommunicationException("no response");
                }

                response = JsonConvert.DeserializeObject<ResponseDetails>(line);
            });

            var writes = new Func<Task>(async () =>
            {
                var writer = new StreamWriter(relayConnection) { AutoFlush = true };
                string line = JsonConvert.SerializeObject(request);
                if (string.IsNullOrEmpty(line))
                {
                    throw new RelayCommunicationException("no content");
                }

                await writer.WriteLineAsync(line);
            });

            await Task.WhenAll(reads(), writes());

            await relayConnection.ShutdownAsync(CancellationToken.None);
            await relayConnection.CloseAsync(CancellationToken.None);
            return response;
        }
    }
}
