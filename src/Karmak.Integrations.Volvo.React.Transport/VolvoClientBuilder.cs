using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Security;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public class VolvoClientBuilder
    {
        private ILogger _logger;
        private IOAuthClient _react5AuthClient;
        private IOAuthClient _react6AuthClient;
        private IOAuthClient _react6PilotAuthClient;
        private HttpClient _httpClient;
        private Uri _volvoUri;
        private string _volvo60BaseUri;
        private string _volvo60PilotBaseUri;
        private Dictionary<string, string> _routes60;
        private IExtendedLoggingClient _extendedLoggingClient;
        private IExtendedLoggingService _extendedLoggingService;
        private string _extendedLoggingModule;
        private string _extendedLoggingApplication;

        public VolvoClientBuilder WithTelemetry(ILogger logger)
        {
            _logger = logger;

            return this;
        }

        public VolvoClientBuilder WithHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            return this;
        }

        /// <summary>
        /// Support Prod and Pilot endpoints/>
        /// </summary>
        /// <param name="volvoUri"></param>
        /// <param name="volvoPilotUri"></param>
        /// <returns></returns>
        public VolvoClientBuilder WithVolvoEnvironment(Uri volvoUri, string volvo60BaseUri, string volvo60PilotBaseUri, Dictionary<string, string> routes60)
        {
            _volvoUri = volvoUri;
            _volvo60BaseUri = volvo60BaseUri;
            _volvo60PilotBaseUri = volvo60PilotBaseUri;
            _routes60 = routes60;
            return this;
        }

        public VolvoClientBuilder WithExtendedLogging(IExtendedLoggingService extendedLoggingService, string module, string application)
        {
            _extendedLoggingService = extendedLoggingService;
            _extendedLoggingModule = module;
            _extendedLoggingApplication = application;

            return this;
        }

        public VolvoClientBuilder WithExtendedLogging(IExtendedLoggingClient extendedLoggingClient)
        {
            _extendedLoggingClient = extendedLoggingClient;

            return this;
        }

        public VolvoClientBuilder WithVolvoOAuth(IOAuthClient react5Auth)
        {
            _react5AuthClient = react5Auth;
            return this;
        }

        public VolvoClientBuilder WithVolvoOAuth20(IOAuthClient react6Auth, IOAuthClient react6PilotAuth)
        {
            _react6AuthClient = react6Auth;
            _react6PilotAuthClient = react6PilotAuth;
            return this;
        }

        public VolvoClient Build()
        {
            return new VolvoClient(
                new Configuration
                {
                    Logger = _logger ?? throw new ArgumentException("Telemetry client must be provided"),
                    VolvoUri = _volvoUri ?? throw new ArgumentException("Volvo environment URI must be provided"),
                    Volvo60BaseUri = _volvo60BaseUri,
                    Volvo60PilotBaseUri = _volvo60PilotBaseUri,
                    HttpClient = _httpClient ?? new HttpClient(),
                    React5AuthClient = _react5AuthClient,
                    React6AuthClient = _react6AuthClient,
                    React6PilotAuthClient = _react6PilotAuthClient,
                    Routes60 = _routes60,
                    ExtendedLogging = new Configuration.ExtendedLoggingConfiguration
                    {
                        Client = BuildExtendedLoggingClient(),
                        Module = _extendedLoggingModule,
                        Application = _extendedLoggingApplication
                    }
                });
        }

        private IExtendedLoggingClient BuildExtendedLoggingClient()
        {
            if (_extendedLoggingClient != null)
            {
                return _extendedLoggingClient;
            }

            if (_extendedLoggingService != null)
            {
                return new ExtendedLoggingClient(_logger, _extendedLoggingService, _extendedLoggingModule, _extendedLoggingApplication);
            }

            return new NullExtendedLoggingClient();
        }
    }
}
