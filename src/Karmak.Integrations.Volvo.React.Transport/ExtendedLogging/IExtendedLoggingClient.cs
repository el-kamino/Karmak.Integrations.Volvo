using Elk.Core.ExtendedLogging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public interface IExtendedLoggingClient {
        Task Execute(string content, string contentDescription);
        Task Execute(string content, string contentDescription, IDictionary<string, string> metadata);
        Task Execute(string content, string contentDescription, string contentType);
        Task Execute(string content, string contentDescription, IDictionary<string, string> metadata, string contentType);

        Task<T> Execute<T>(string content, string contentDescription) where T : ExtendedLogReference;
        Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata) where T : ExtendedLogReference;
        Task<T> Execute<T>(string content, string contentDescription, string contentType) where T : ExtendedLogReference;
        Task<T> Execute<T>(string content, string contentDescription, IDictionary<string, string> metadata, string contentType) where T:ExtendedLogReference;
    }
}