using System;
using System.Threading.Tasks;
using System.Xml;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class PassThroughEncryptionHandler : IEncryptionHandler
    {
        private readonly ILogger<PassThroughEncryptionHandler> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;

        public PassThroughEncryptionHandler(ILogger<PassThroughEncryptionHandler> logger, IExtendedLoggingClient extendedLoggingClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
        }

        public async Task<(bool success, XmlDocument response)> Execute(XmlDocument encryptedRequest, Func<XmlDocument, Task<(bool success, SoapEnvelope response)>> requestHandler)
        {
            await _extendedLoggingClient.Execute(encryptedRequest.OuterXml, "Raw push request", VolvoLogContentType.XML);

            _logger.LogInformation("Triggering request handler.");
            var (success, response) = await requestHandler.Invoke(encryptedRequest);
            _logger.LogInformation("Request handler finished.");

            var responseXml = ToXml.Document(response);
            await _extendedLoggingClient.Execute(responseXml.OuterXml, "Push response", VolvoLogContentType.XML);

            return (success, responseXml);
        }
    }
}
