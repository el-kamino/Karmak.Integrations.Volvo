namespace Karmak.Integrations.Volvo.Dcds.Auth
{
    public class TokenResponse
    {
        [Newtonsoft.Json.JsonProperty("access_token")]
        public string? AccessToken { get; private set; }

        [Newtonsoft.Json.JsonProperty("expires_in")]
        public int ExpiresIn { get; private set; }
    }
}
