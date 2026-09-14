using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Extensions;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomerInfo = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate
{
    internal class ProcessCustomerUpdate : IProcessCustomerUpdate
    {
        private readonly ILogger _logger;
        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly IVolvoClient _volvoClient;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IOptions<ProcessorOptions> _options;
        private readonly ISoapRequestFactory _soapRequestFactory;

        public ProcessCustomerUpdate(
            ILogger<ProcessCustomerUpdate> logger,
            IVolvoExtendedLoggingService extendedLoggingService,
            IVolvoClient volvoClient,
            ISettingsProvider settingsProvider,
            IOptions<ProcessorOptions> options,
            ISoapRequestFactory soapRequestFactory)
        {
            _logger = logger;
            _extendedLoggingService = extendedLoggingService;
            _volvoClient = volvoClient;
            _settingsProvider = settingsProvider;
            _options = options;
            _soapRequestFactory = soapRequestFactory;
        }

        public async Task Execute(CustomerInfo customerInfo, string newOrHistorical)
        {
            var metadata = new Dictionary<string, string>(customerInfo.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = newOrHistorical.Equals("H") ? EntityTypes.RetransmittedCustomerUpdate : EntityTypes.CustomerUpdate
            };

            await _extendedLoggingService.LogInboundDto(customerInfo, metadata);

            if (!customerInfo.IsValid(out string errMsg))
            {
                var errMeta = BuildErrorMetadata(metadata, Metrics.InvalidEntity, errMsg);
                throw new InvalidEntityException(typeof(CustomerInfo), errMsg, errMeta);
            }

            var settings = await GetValidatedVolvoSettingsAsync(metadata);
            var address = new ProcessCustomerUpdateWebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var msgList = new CreateProcessCustomerUpdateTransmission(_options, _soapRequestFactory, settings, customerInfo, address, newOrHistorical).BuildMessageList();

            await SendMessagesToVolvo(customerInfo, metadata, msgList);
        }

        private async Task SendMessagesToVolvo(CustomerInfo customerInfo, Dictionary<string, string> metadata, List<SoapEnvelope> msgList)
        {
            if (msgList.Count > 0)
            {
                foreach (SoapEnvelope se in msgList)
                {
                    await _extendedLoggingService.LogOutBoundMessage(se.ToXDocument().ToString(), metadata);

                    (await _volvoClient.OAuthSendSoapAsync(new VolvoSoapRequest(se), metadata))
                        .RaiseAlertOnSoapFault(metadata, _logger)
                        .ThrowIfNotSuccessful();
                }
                _logger.LogInformationWithMetadata($"Finished Processing Customer Update {customerInfo.Customer.CustomerKey}", metadata);
            }
            else
            {
                _logger.LogInformationWithMetadata($"Customer {customerInfo.Customer.CustomerKey} has no associated VIN or Volvo Pass Rewards; no messages sent", metadata);
            }
        }

        private async Task<VolvoSettings> GetValidatedVolvoSettingsAsync(Dictionary<string, string> metadata)
        {
            var settings = await _settingsProvider.GetSettingsAsync();

            if (!settings.IsValid(out string errMsg))
            {
                var errMeta = BuildErrorMetadata(metadata, Metrics.InvalidSettings, errMsg);
                _logger.LogInformationWithMetadata("Found Invalid Settings", errMeta);
                throw new InvalidSettingsException(errMsg);
            }

            return settings;
        }

        private Dictionary<string, string> BuildErrorMetadata(
            Dictionary<string, string> metadata, string metric, string errorMessage) => new Dictionary<string, string>(metadata)
            {
                [TelemetryKeys.Metric] = metric,
                [TelemetryKeys.ErrorMessage] = errorMessage
            };
    }
}
