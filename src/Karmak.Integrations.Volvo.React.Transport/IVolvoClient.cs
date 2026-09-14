using Karmak.Integrations.Volvo.React.Transport.Soap;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public interface IVolvoClient {
        Task<SoapResult> OAuthSendSoapAsync(VolvoSoapRequest request, IDictionary<string, string> metadata);
        Task OAuth2_0SendJsonAsync(string jsonPayload, bool isPilot, string entityType, IDictionary<string, string> metadata);
    }
}