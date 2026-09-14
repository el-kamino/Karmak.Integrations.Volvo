using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.Transport.Security
{
    public class OAuthClient : IOAuthClient
    {
        private string _token = null;
        private DateTime? _expiration = null;

        private readonly IHttpClientFactory  _httpClientFactory;
        private readonly OAuthClientOptions _options;

        public OAuthClient(IHttpClientFactory httpClientFactory, IOptions<OAuthClientOptions> options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
        }

        public async Task<string> GetAuthTokenAsync()
        {
            if (_token is null || DateTime.UtcNow > _expiration)
            {
                TokenResponse tokenData = await GetVolvoOAuthTokenAsync();
                _token = tokenData.AccessToken;

                var parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(_token);
                var now = DateTime.UtcNow;
                TimeSpan duration = (parsedToken.ValidTo - now) / 2;
                _expiration = now + duration;
            }

            return _token;
        }

        private async Task<TokenResponse> GetVolvoOAuthTokenAsync()
        {
            var payload = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _options.ClientId,
                ["client_secret"] = _options.ClientSecret,
            };

            if (!string.IsNullOrWhiteSpace(_options.TokenResource))
            {
                payload["resource"] = _options.TokenResource;
            }
            else
            {
                payload["scope"] = _options.Scope;
            }

            var content = new FormUrlEncodedContent(payload);
            using(var httpClient = _httpClientFactory.CreateClient())
            using (var response = await httpClient.PostAsync(_options.TokenUri, content))
            {
                response.EnsureSuccessStatusCode();

                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var streamReader = new StreamReader(responseStream))
                using (var jtr = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<TokenResponse>(jtr);
                    return data;
                }
            }
        }

        private class TokenResponse
        {
            [JsonProperty("access_token")]
            public string AccessToken { get; private set; }

            [JsonProperty("expires_in")]
            public int ExpiresIn { get; private set; }
        }
    }
}
