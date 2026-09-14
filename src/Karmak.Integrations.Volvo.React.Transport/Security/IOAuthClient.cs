namespace Karmak.Integrations.Volvo.React.Transport.Security
{
    public interface IOAuthClient
    {
        Task<string> GetAuthTokenAsync();
    }
}
