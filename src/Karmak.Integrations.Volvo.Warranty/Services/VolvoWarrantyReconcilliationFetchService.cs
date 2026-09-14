using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SoapMessageAddress = Karmak.Integrations.Volvo.React.Transport.Soap.SoapMessageAddress;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class VolvoWarrantyReconcilliationFetchService : IVolvoWarrantyReconcilliationFetchService
    {
        private readonly ILogger<VolvoWarrantyReconcilliationFetchService> _logger;
        private readonly IShowServiceProcessingAdvisoryHandler _handler;
        private readonly ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType> _translator;
        private readonly IVolvoClient _volvoClient;
        private readonly ISettingsProvider _volvoSettingsClient;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly ISoapRequestFactory _soapRequestFactory;

        public VolvoWarrantyReconcilliationFetchService(
            ILogger<VolvoWarrantyReconcilliationFetchService> logger,
            ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType> translator,
            IVolvoClient volvoClient,
            ISettingsProvider volvoSettingsClient,
            IShowServiceProcessingAdvisoryHandler handler,
            IExtendedLoggingClient extendedLoggingClient,
            ISoapRequestFactory soapRequestFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _volvoClient = volvoClient ?? throw new ArgumentNullException(nameof(volvoClient));
            _volvoSettingsClient = volvoSettingsClient ?? throw new ArgumentNullException(nameof(volvoSettingsClient));
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _extendedLoggingClient = extendedLoggingClient ?? throw new ArgumentNullException(nameof(extendedLoggingClient));
            _soapRequestFactory = soapRequestFactory ?? throw new ArgumentNullException(nameof(soapRequestFactory));
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

        public async Task<(bool success, SoapResult response)> TriggerReconciliationFetch(ReconciliationFetchRequest dateRange)
        {
            var volvoSettings = await _volvoSettingsClient.GetSettingsAsync();

            if (WarrantyIsDisabled(volvoSettings.InterfaceOptions))
            {
                _logger.LogInformationWithMetadata("Warranty is disabled in interface options. Aborting Volvo reconciliation fetch.",
                new Dictionary<string, string>
                {
                    ["volvoSettings.InterfaceOptions.PACode"] = volvoSettings.InterfaceOptions.PaCode,
                    ["volvoSettings.RegionSettings.CountryCode"] = volvoSettings.RegionSettings.CountryCode,
                    ["volvoSettings.RegionSettings.LanguageCode"] = volvoSettings.RegionSettings.LanguageCode,
                    ["volvoSettings.RegionSettings.CurrencyCode"] = volvoSettings.RegionSettings.CurrencyCode
                });
                return (true, null);
            }

            _logger.LogInformationWithMetadata("Beginning Volvo Reconciliation fetch.",
                new Dictionary<string, string>
                {
                    ["volvoSettings.InterfaceOptions.PACode"] = volvoSettings.InterfaceOptions.PaCode,
                    ["volvoSettings.RegionSettings.CountryCode"] = volvoSettings.RegionSettings.CountryCode,
                    ["volvoSettings.RegionSettings.LanguageCode"] = volvoSettings.RegionSettings.LanguageCode,
                    ["volvoSettings.RegionSettings.CurrencyCode"] = volvoSettings.RegionSettings.CurrencyCode,
                    ["volvoSettings.DealerServiceProviderSettings.SoftwareName"] =
                        volvoSettings.DealerServiceProviderSettings.SoftwareName,
                    ["volvoSettings.DealerServiceProviderSettings.Code"] =
                        volvoSettings.DealerServiceProviderSettings.Code,
                    ["volvoSettings.DealerServiceProviderSettings.ShortCode"] =
                        volvoSettings.DealerServiceProviderSettings.ShortCode
                });

            var translation = _translator.Translate(new GetClaimReconciliationTranslatorArguments()
            {
                Source = dateRange,
                Settings = volvoSettings
            });
            var request = CreateRequest(translation, volvoSettings);
            var response = await _volvoClient.OAuthSendSoapAsync(request, new Dictionary<string, string>());

            if (response is SoapResult.Success)
            {
                var result = await ProcessResponse(response as SoapResult.Success);
                return (result.Succeeded, response);
            }
            else
            {
                LogErrorResponse(response);
                return (false, response);
            }
        }

        private void LogErrorResponse(SoapResult response)
        {
            switch (response)
            {
                case SoapResult.Failure failure:
                    _logger.LogInformationWithMetadata($"Received {nameof(SoapResult.Failure)} from reconciliation fetch.",
                        new Dictionary<string, string> { { "Response Status", failure.HttpStatus.ToString() } });
                    break;
                case SoapResult.InvalidSignature invalidSignature:
                    _logger.LogInformation($"Received {nameof(SoapResult.InvalidSignature)} from reconciliation fetch.");
                    break;
                case SoapResult.Error error:
                    if (error.Exception != null)
                    {
                        _logger.LogInformationWithMetadata($"Received {nameof(SoapResult.Error)} from reconciliation fetch.",
                            new Dictionary<string, string> { { "Response.Exception", error.Exception.Message } });
                    }
                    else
                    {
                        _logger.LogInformation($"Received {nameof(SoapResult.Error)} from reconciliation fetch.");
                    }
                    break;
            }
        }

        private async Task<Result> ProcessResponse(SoapResult.Success successResponse)
        {
            if (!TryExtractShowServiceProcessingAdvisoryType(successResponse.Response, out var showServiceProcessingAdvisory))
            {
                throw new ReconcilliationFetchException(
                $"{nameof(TryExtractShowServiceProcessingAdvisoryType)} did not find a valid message in the body.");
            }

            await _extendedLoggingClient.Execute(JsonConvert.SerializeObject(showServiceProcessingAdvisory), "Extracted Reconciliation Body", VolvoLogContentType.XML);

            if (showServiceProcessingAdvisory.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory == null)
            {
                _logger.LogInformationWithMetadata("Received an empty reconciliation update payload.",
                    new Dictionary<string, string> { { "DealerCode", showServiceProcessingAdvisory.ApplicationArea.Destination.DealerNumberID.Value } });
                return Result.Success();
            }

            var result = await _handler.HandleAsync(showServiceProcessingAdvisory);
            if (!result.Succeeded)
            {
                throw new ReconcilliationFetchException(
                    $"Failed to process manual reconciliation fetch. Error: {result.FaultCode} {result.FaultDetails} {result.FaultReason}");
            }

            return result;
        }

        private VolvoSoapRequest CreateRequest(
            GetServiceProcessingAdvisoryType getServiceProcessingAdvisoryType,
            VolvoSettings settings)
        {
            return new VolvoSoapRequest(_soapRequestFactory.CreateProcessRequest(
                new SoapMessageAddress
                {
                    To = ConstantSettings.ClaimReconciliationRequest,
                    Action = ConstantSettings.StarProcessMessageAction,
                    TargetService = ConstantSettings.VolvoGetServiceProcessingAdvisory,
                    TargetServiceVersion = ConstantSettings.VolvoOneWarrantySystemVersion,
                    SiteCode = settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode
                },
                getServiceProcessingAdvisoryType));
        }

        private bool WarrantyIsDisabled(InterfaceOptions interfaceOptions)
        {
            if (ReactAndWarrantyEnabledAreNull(interfaceOptions))
            {
                return false;
            }
            else
            {
                return (bool)(!interfaceOptions.WarrantyEnabled);
            }
        }

        private bool ReactAndWarrantyEnabledAreNull(InterfaceOptions interfaceOptions)
        {
            return !(interfaceOptions.ReactEnabled.HasValue || interfaceOptions.WarrantyEnabled.HasValue);
        }
    }
}
