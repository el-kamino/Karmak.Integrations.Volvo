using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Soap;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class VolvoPushUpdateService : IVolvoPushUpdateService
    {
        private readonly ILogger<VolvoPushUpdateService> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly IEncryptionHandler _encryptionHandler;
        private readonly IWarrantyElkContextRetriever _authenticationClient;
        private readonly IShowServiceProcessingAdvisoryHandler _handler;

        public VolvoPushUpdateService(
            IEncryptionHandler encryptionHandler,
            IExtendedLoggingClient extendedLoggingClient,
            ILogger<VolvoPushUpdateService> logger,
            IWarrantyElkContextRetriever authenticationClient,
            IShowServiceProcessingAdvisoryHandler handler)
        {
            _encryptionHandler = encryptionHandler ?? throw new ArgumentNullException(nameof(encryptionHandler));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authenticationClient = authenticationClient ?? throw new ArgumentNullException(nameof(authenticationClient));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public async Task<(bool success, XmlDocument response)> HandleUpdate(XmlDocument request)
        {
            return await _encryptionHandler.Execute(request, async decryptedRequest =>
            {
                if (!TryExtractShowServiceProcessingAdvisoryType(decryptedRequest,
                    out var showServiceProcessingAdvisory))
                    return (false, ResponseFactory.Fault());

                if (!TryExtractPACode(showServiceProcessingAdvisory, out var paCode))
                    return (false, ResponseFactory.Fault());

                ElkContext context = null;

                try
                {
                    context = await _authenticationClient.GetElkContextAsync(paCode);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to find a context associated with PACode {paCode}.");
                    return (false, ResponseFactory.Fault());
                }

                // The context lookup can return null without throwing (see FetchController, which guards
                // the same call with a 401). The original faulted on any lookup failure, so guard null here
                // rather than processing under a null/ambient Elk context.
                if (context == null)
                {
                    _logger.LogError($"Failed to find a context associated with PACode {paCode}.");
                    return (false, ResponseFactory.Fault());
                }

                _logger.LogInformation("Finished fetching Elk Context.");

                SetPaCode(paCode, ref showServiceProcessingAdvisory);
                await ImplicitElkContext.WithCurrentAsync(context, () => _extendedLoggingClient.Execute(ToXml.Document(showServiceProcessingAdvisory).OuterXml, "Clean push message", VolvoLogContentType.XML));
                var result = await ImplicitElkContext.WithCurrentAsync(context, () => _handler.HandleAsync(showServiceProcessingAdvisory));

                if (result.Succeeded) return (true, ResponseFactory.BuildEnvelope(new PutMessageResponse()));
                return (false, ResponseFactory.Fault(result.FaultDetails, result.FaultCode, result.FaultReason));
            });
        }

        private static void SetPaCode(string paCode, ref ShowServiceProcessingAdvisoryType request)
        {
            request.ApplicationArea.Destination.DealerNumberID.Value = paCode;
        }

        private static bool TryExtractPACode(ShowServiceProcessingAdvisoryType type, out string paCode)
        {
            paCode = type?.ApplicationArea?.Destination?.DealerNumberID?.Value?.Replace("!", "");
            return !string.IsNullOrWhiteSpace(paCode);
        }

        private bool TryExtractShowServiceProcessingAdvisoryType(
            XmlDocument request,
            out ShowServiceProcessingAdvisoryType showServiceProcessingAdvisory)
        {
            var list = request.GetElementsByTagName("ShowServiceProcessingAdvisory", XmlNamespaces.StarUrl);

            showServiceProcessingAdvisory = null;

            if (list == null || list.Count != 1)
            {
                _logger.LogError($"{nameof(TryExtractShowServiceProcessingAdvisoryType)} did not find a valid message in the body.");

                return false;
            }

            var processingAdvisory = list[0];

            try
            {
                var serializer = new XmlSerializer(typeof(ShowServiceProcessingAdvisoryType));

                using (XmlReader reader = new XmlNodeReader(processingAdvisory))
                {
                    showServiceProcessingAdvisory = (ShowServiceProcessingAdvisoryType)serializer.Deserialize(reader);
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return false;
            }
        }
    }
}
