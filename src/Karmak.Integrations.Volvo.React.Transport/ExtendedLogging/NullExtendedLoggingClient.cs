using Elk.Core.ExtendedLogging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public class NullExtendedLoggingClient : IExtendedLoggingClient {
        public Task Execute(string content, string contentDescription) {
            return Task.CompletedTask;
        }

        public Task Execute(string content, string contentDescription, IDictionary<string, string> metadata) {
            return Task.CompletedTask;
        }

        public Task Execute(string content, string contentDescription, string contentType) {
            return Task.CompletedTask;
        }

        public Task Execute(string content, string contentDescription, IDictionary<string, string> metadata, string contentType) {
            return Task.CompletedTask;
        }

        public Task<T> Execute<T>(string content, string contentDescription) where T : ExtendedLogReference {
            return Task.FromResult<T>(null);
        }

        public Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata) where T : ExtendedLogReference {
            return Task.FromResult<T>(null);
        }

        public Task<T> Execute<T>(string content, string contentDescription, string contentType) where T : ExtendedLogReference {
            return Task.FromResult<T>(null);
        }

        public Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata, string contentType) where T : ExtendedLogReference {
            return Task.FromResult<T>(null);
        }
    }
}