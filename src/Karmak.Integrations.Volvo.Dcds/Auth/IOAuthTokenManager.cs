namespace Karmak.Integrations.Volvo.Dcds.Auth
{
    public interface IOAuthTokenManager
    {
        string ClientId { get; }

        Task<string> GetInboundToken();
        Task<string> GetOutboundToken();
    }
}