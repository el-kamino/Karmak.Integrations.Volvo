using Elk.Core.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using System.Text;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.Utils {
    public class StubExtendedLoggingClient : IExtendedLoggingClient {
        public const string Module = "FakeModule";
        public const string Application = "FakeApplication";

        public IList<ExtendedLoggingRequest> LogEvents { get; } = new List<ExtendedLoggingRequest>();

        public Task Execute(string content, string contentDescription) {
            return Execute(content, contentDescription, new Dictionary<string, string>(), null);
        }

        public Task Execute(string content, string contentDescription, string contentType) {
            return Execute(content, contentDescription, new Dictionary<string, string>(), contentType);
        }

        public Task Execute(string content, string contentDescription, IDictionary<string, string> metadata) {
            return Execute(content, contentDescription, metadata, null);
        }

        public Task Execute(string content, string contentDescription, IDictionary<string, string> metadata, string contentType) {
            LogEvents.Add(new ExtendedLoggingRequest {
                Module = Module,
                Application = Application,
                Content = Encoding.Default.GetBytes(content),
                Metadata = metadata,
                ContentType = contentType
            });
            return Task.CompletedTask;
        }

        public Task<T> Execute<T>(string content, string contentDescription) where T : ExtendedLogReference {
            throw new System.NotImplementedException();
        }

        public Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata) where T : ExtendedLogReference {
            throw new System.NotImplementedException();
        }

        public Task<T> Execute<T>(string content, string contentDescription, string contentType) where T : ExtendedLogReference {
            throw new System.NotImplementedException();
        }

        public Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata, string contentType) where T : ExtendedLogReference {
            throw new System.NotImplementedException();
        }
    }
}