using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public class NullVolvoClient : IVolvoClient
    {
        private readonly ILogger _telemetryClient;

        public NullVolvoClient(ILogger telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public Task OAuth2_0SendJsonAsync(string jsonPayload, bool isPilot, string entityType, IDictionary<string, string> metadata)
        {
            _telemetryClient.LogInformation("Sending to Volvo disabled. Dropping transmission.");
            return Task.CompletedTask;
        }

        public Task<SoapResult> OAuthSendSoapAsync(VolvoSoapRequest request, IDictionary<string, string> metadata)
        {
            _telemetryClient.LogInformationWithMetadata("Sending to Volvo disabled. Dropping transmission.", metadata);
            return Task.FromResult<SoapResult>(new SoapResult.Success(null));
        }
    }
}