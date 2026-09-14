using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Xml;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public class OasisClient : IOasisClient
    {
        private readonly HttpClient _http;
        private readonly IOAuthClient _authClient;
        private readonly ILogger _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;

        public OasisClient(
            HttpClient http,
            [FromKeyedServices("React5Auth")] IOAuthClient authClient,
            ILogger<OasisClient> logger,
            IExtendedLoggingClient extendedLoggingClient)
        {
            _http = http;
            _authClient = authClient;
            _logger = logger;
            _extendedLoggingClient = extendedLoggingClient;
        }

        public async Task<OasisResponseRest> SendAsync(string oasisUri, OasisRequest request, string karmakAccountNumber = null)
        {
            _logger.LogInformationWithMetadata(
                "Sending OasisRequest to Volvo",
                new Dictionary<string, string>
                {
                    ["VIN"] = request.Vin,
                    ["PaCode"] = request.PaCode.ToString(),
                });

            try
            {
                using (var response = await PostAsync(new Uri(oasisUri), BuildHttpContent(request)))
                {
                    var body = await response.Content.ReadAsStringAsync();
                    await _extendedLoggingClient.Execute(body, "OASIS Response", new Dictionary<string, string>
                    {
                        ["vin"] = request.Vin,
                        ["Request"] = request.ToXml()
                    });

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformationWithMetadata(
                            "Successfully submitted OasisRequest to Volvo",
                            new Dictionary<string, string>
                            {
                                [TelemetryKeys.KarmakAccountNumber] = karmakAccountNumber,
                                [TelemetryKeys.Module] = TelemetryValues.Oasis,
                                [TelemetryKeys.BusinessProcess] = "Submitted Oasis Request",
                                [TelemetryKeys.OEM] = TelemetryValues.Volvo

                            });

                        return new OasisResponseRest
                        {
                            Payload = SerializationUtils.DeserializeXmlToJson(body)
                        };
                    }

                    _logger.LogInformationWithMetadata(
                        "Failed to send OasisRequest to Volvo",
                        new Dictionary<string, string>
                        {
                            ["HttpStatus"] = response.StatusCode.ToString()
                        });

                    return BuildCriticalErrorPayload($"HTTP-{response.StatusCode}", "Unexpected System Error");
                }
            }
            catch (Exception e) when (e is HttpRequestException || e is XmlException)
            {
                _logger.LogError(e, "exception retrieving oasis data");
                return BuildCriticalErrorPayload("SYS-500", "Unexpected System Error");
            }
        }

        private OasisResponseRest BuildCriticalErrorPayload(string errorNumber, string errorMessage)
        {
            var criticalError = new
            {
                CriticalErrorInfo = new
                {
                    ErrorNumber = errorNumber,
                    ErrorMessage = errorMessage
                }
            };

            return new OasisResponseRest
            {
                Payload = JsonConvert.SerializeObject(criticalError)
            };
        }

        public virtual async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content)
        {
            var authToken = await _authClient.GetAuthTokenAsync();
            var message = new HttpRequestMessage
            {
                Content = content,
                RequestUri = uri,
                Method = HttpMethod.Post
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            return await _http.SendAsync(message);
        }

        private HttpContent BuildHttpContent(OasisRequest request)
        {
            var xml = request.ToXml();
            var content = new StringContent(xml, Encoding.UTF8, MediaTypeNames.Text.Xml);
            return content;
        }
    }
}
