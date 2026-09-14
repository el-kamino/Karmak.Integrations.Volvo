using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Karmak.Integrations.Volvo.Dcds.Auth
{
    public class OAuthTokenManager : IOAuthTokenManager
    {
        public string ClientId { get => _clientId; }

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tokenUri;
        private readonly string _inboundScope;
        private readonly string _outboundScope;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializer _jsonSerializer;

        private TokenResponse? _inboundToken;
        private DateTime _inboundTokenExpiration;
        private TokenResponse? _outboundToken;
        private DateTime _outboundTokenExpiration;

        private readonly int TOKEN_EXPIRY_BUFFER_SEC = 30;

        public OAuthTokenManager(HttpClient httpClient, IOptions<OAuthTokenManagerOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options?.Value);

            var settings = options.Value;
            _clientId = settings.ClientId;
            _clientSecret = settings.ClientSecret;
            _tokenUri = settings.TokenUri;
            _inboundScope = settings.InboundScope;
            _outboundScope = settings.OutboundScope;
            _httpClient = httpClient;
            _jsonSerializer = new JsonSerializer();
            _inboundToken = null;
            _inboundTokenExpiration = DateTime.MinValue;
            _outboundToken = null;
            _outboundTokenExpiration = DateTime.MinValue;
        }

        public async Task<string> GetInboundToken()
        {
            // token not set or is expired
            if (_inboundToken is null || DateTime.UtcNow >= _inboundTokenExpiration)
                await GetInboundTokenResponse();

            return _inboundToken!.AccessToken!;
        }

        public async Task<string> GetOutboundToken()
        {
            // token not set or is expired
            if (_outboundToken is null || DateTime.UtcNow >= _outboundTokenExpiration)
                await GetOutboundTokenResponse();

            return _outboundToken!.AccessToken!;
        }

        private async Task GetInboundTokenResponse()
        {
            _inboundTokenExpiration = DateTime.UtcNow;
            if (_tokenUri == null)
                return;

            var request = new HttpRequestMessage(HttpMethod.Post, _tokenUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Base64Encode($"{_clientId}:{_clientSecret}"));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = _inboundScope
            });

            using (var response = await _httpClient.SendAsync(request, new CancellationToken()))
            using (var ensureResponse = response.EnsureSuccessStatusCode())
            using (var responseStream = await ensureResponse.Content.ReadAsStreamAsync())
            using (var responseReader = new StreamReader(responseStream))
            {
                _inboundToken = (TokenResponse?)_jsonSerializer.Deserialize(responseReader, typeof(TokenResponse));
            }
            _inboundTokenExpiration = _inboundTokenExpiration.AddSeconds(_inboundToken!.ExpiresIn - TOKEN_EXPIRY_BUFFER_SEC);
        }

        private async Task GetOutboundTokenResponse()
        {
            _outboundTokenExpiration = DateTime.UtcNow;
            if (_tokenUri == null)
                return;

            var request = new HttpRequestMessage(HttpMethod.Post, _tokenUri);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Base64Encode($"{_clientId}:{_clientSecret}"));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = _outboundScope
            });

            using (var response = await _httpClient.SendAsync(request, new CancellationToken()))
            using (var ensureResponse = response.EnsureSuccessStatusCode())
            using (var responseStream = await ensureResponse.Content.ReadAsStreamAsync())
            using (var responseReader = new StreamReader(responseStream))
            {
                _outboundToken = (TokenResponse?)_jsonSerializer.Deserialize(responseReader, typeof(TokenResponse));
            }
            _outboundTokenExpiration = _outboundTokenExpiration.AddSeconds(_outboundToken!.ExpiresIn - TOKEN_EXPIRY_BUFFER_SEC);
        }

        private static string Base64Encode(string str) => Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
    }
}
