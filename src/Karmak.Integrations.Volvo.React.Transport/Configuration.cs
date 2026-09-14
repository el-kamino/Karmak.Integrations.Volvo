using Microsoft.Extensions.Logging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Security;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public class Configuration
    {
        public ILogger Logger { get; set; }
        public HttpClient HttpClient { get; set; }
        public Uri VolvoUri { get; set; }
        public string Volvo60BaseUri { get; set; }
        public string Volvo60PilotBaseUri { get; set; }
        public ExtendedLoggingConfiguration ExtendedLogging { get; set; }
        public IOAuthClient React5AuthClient { get; set; }
        public IOAuthClient React6AuthClient { get; set; }
        public IOAuthClient React6PilotAuthClient { get; set; }
        public Dictionary<string, string> Routes60 { get; set; }

        public class ExtendedLoggingConfiguration
        {
            public IExtendedLoggingClient Client { get; set; }
            public string Module { get; set; }
            public string Application { get; set; }
        }
    }
}
