using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Security;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text;
using System.Xml;
using System.Net.Http.Headers;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public class VolvoClient : IVolvoClient
    {
        private readonly ILogger _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly IOAuthClient _react5AuthClient;
        private readonly IOAuthClient _react6AuthClient;
        private readonly IOAuthClient _react6PilotAuthClient;
        private readonly HttpClient _httpClient;
        private readonly Uri _volvoUri;
        private readonly Uri _volvo60BaseUri;
        private readonly Uri _volvo60PilotBaseUri;
        private readonly Dictionary<string, string> _routes6_0;

        public VolvoClient(Configuration configuration)
        {
            _logger = configuration.Logger;
            _extendedLoggingClient = configuration.ExtendedLogging.Client;
            _react5AuthClient = configuration.React5AuthClient;
            _react6AuthClient = configuration.React6AuthClient;
            _react6PilotAuthClient = configuration.React6PilotAuthClient;
            _httpClient = configuration.HttpClient;
            _volvoUri = configuration.VolvoUri;
            _volvo60BaseUri = new Uri(configuration.Volvo60BaseUri);
            _volvo60PilotBaseUri = new Uri(configuration.Volvo60PilotBaseUri);
            _routes6_0 = configuration.Routes60;
        }

        public static VolvoClientBuilder Builder()
        {
            return new VolvoClientBuilder();
        }

        public async Task OAuth2_0SendJsonAsync(string jsonPayload, bool isPilot, string entityType, IDictionary<string, string> metadata)
        {
            var fullRoute = new Uri(isPilot ? _volvo60PilotBaseUri : _volvo60BaseUri, _routes6_0[entityType]);
            var requestMetadata = GetMetaData(metadata, fullRoute.ToString(), isPilot);
            await _extendedLoggingClient.Execute(jsonPayload, "Volvo JSON request OAuth 2.0", requestMetadata);

            string authToken = null;

            if (isPilot)
            {
                authToken = await _react6PilotAuthClient.GetAuthTokenAsync();
            }
            else
            {
                authToken = await _react6AuthClient.GetAuthTokenAsync();
            }

            HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, fullRoute);
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            requestMsg.Content = new StringContent(jsonPayload, Encoding.UTF8, MediaTypeNames.Application.Json);

            _logger.LogInformationWithMetadata("Sending request to Volvo OAuth 2.0", requestMetadata);

            try
            {
                using (HttpResponseMessage response = await _httpClient.SendAsync(requestMsg))
                {
                    var body = await response.Content.ReadAsStringAsync();

                    _logger.LogInformationWithMetadata("Received Volvo response OAuth 2.0",
                        new Dictionary<string, string>(requestMetadata)
                        {{ "Volvo.Response.Status", response.StatusCode.ToString() }});
                    await _extendedLoggingClient.Execute(body, "Volvo JSON response OAuth 2.0", requestMetadata);
                }
            }
            catch (Exception exception)
            {
                _logger.LogInformationWithMetadata("Error occurred while sending Volvo OAuth 2.0 request",
                    new Dictionary<string, string>(requestMetadata)
                    {{ "Exception.Message", exception.Message }});
            }
        }

        #region Soap Methods
        public async Task<SoapResult> OAuthSendSoapAsync(VolvoSoapRequest request, IDictionary<string, string> metadata)
        {
            var requestMetadata = GetMetaData(metadata, _volvoUri.ToString(),  false);

            await _extendedLoggingClient.Execute(ToXml.Document(request.Body).OuterXml, "Raw Volvo request", requestMetadata);
            string authToken = await _react5AuthClient.GetAuthTokenAsync();

            HttpRequestMessage requestMsg = new HttpRequestMessage(HttpMethod.Post, _volvoUri);
            requestMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

            var content = ToHttpContent(request);
            requestMsg.Content = content;

            _logger.LogInformationWithMetadata("Sending request to Volvo using OAuth", requestMetadata);

            HttpResponseMessage response = null;
            string body = null;
            try
            {
                response = await _httpClient.SendAsync(requestMsg);
                body = await response.Content.ReadAsStringAsync();

                _logger.LogInformationWithMetadata("Received response from Volvo using OAuth",
                    new Dictionary<string, string>(requestMetadata)
                    {
                       { "Volvo.Response.Status", response.StatusCode.ToString() }
                    });
                await _extendedLoggingClient.Execute(body, "Volvo response using OAuth", requestMetadata);

                if (response.IsSuccessStatusCode)
                {
                    return new SoapResult.Success(ToXmlDocument(body));
                }

                return new SoapResult.Failure(response.StatusCode, ToXmlDocument(body));
            }
            catch (Exception exception) when (exception is HttpRequestException || exception is XmlException)
            {
                return new SoapResult.Error
                {
                    Exception = exception,
                    Response = body
                };
            }
            finally
            {
                response?.Dispose();
            }
        }

        private static HttpContent ToHttpContent(VolvoSoapRequest request)
        {
            var content = new StringContent(ToXml.Document(request.Body).OuterXml, Encoding.UTF8, MediaTypeNames.Application.Soap);

            foreach (var kv in request.Headers)
            {
                content.Headers.Add(kv.Key, kv.Value);
            }

            return content;
        }

        private static XmlDocument ToXmlDocument(string body)
        {
            var response = new XmlDocument { PreserveWhitespace = true };
            response.LoadXml(body);
            return response;
        }

        private Dictionary<string, string> GetMetaData(IDictionary<string, string>  metaData, string uri, bool isPilot)
        {
            return new Dictionary<string, string>(metaData)
            {
                { "Volvo.Uri", uri}
            };
        }
    }
    #endregion

}
