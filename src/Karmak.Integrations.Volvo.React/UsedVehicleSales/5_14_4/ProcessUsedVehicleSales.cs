using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Extensions;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using Karmak.Integrations.Volvo.React.Validators.UsedVehicleSales;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebServiceAddress = Elk.Integrations.Volvo.Core.Transport.Soap.V5_14_4.ProcessRetailDeliveryReportingWebServiceAddress;

namespace Karmak.Integrations.Volvo.React.UsedVehicleSales.V5_14_4
{
    internal class ProcessUsedVehicleSales : IProcessUsedVehicleSales
    {
        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly ILogger _logger;
        private readonly ISettingsProvider _settingsProvider;
        private readonly IVolvoClient _volvoClient;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly ProcessorOptions _options;

        public ProcessUsedVehicleSales(
          IVolvoExtendedLoggingService extendedLoggingService,
            ISettingsProvider settingsProvider,
            ILogger<ProcessUsedVehicleSales> logger,
            ISoapRequestFactory soapRequestFactory,
            IVolvoClient volvoClient,
            IOptions<ProcessorOptions> options)
        {
            _extendedLoggingService = extendedLoggingService;
            _settingsProvider = settingsProvider;
            _logger = logger;
            _soapRequestFactory = soapRequestFactory;
            _volvoClient = volvoClient;
            _options = options.Value;
        }

        public async Task Execute(VehicleSalesOrder vehicleSalesOrder, string newOrHistorical)
        {
            var metadata = new Dictionary<string, string>(vehicleSalesOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = newOrHistorical.Equals("H") ? EntityTypes.RetransmittedVehicleSale : EntityTypes.VehicleSale
            };

            await _extendedLoggingService.LogInboundDto(vehicleSalesOrder, metadata);

            if (vehicleSalesOrder.HasNoUsedVehicles())
            {
                _logger.LogInformationWithMetadata("No Used Vehicles Found, Not Processing Vehicle Sales Order.", metadata);
                return;
            }

            if (!vehicleSalesOrder.IsValid(out var errMsg))
            {
                var errMeta = BuildErrorMetadata(metadata, Metrics.InvalidEntity, errMsg);
                throw new InvalidEntityException(typeof(VehicleSalesOrder), errMsg, errMeta);
            }
            
            var settings = await GetValidatedVolvoSettingsAsync(metadata);
            var msg = new CreateProcessRetailDeliveryReportingTransmission(_options, settings, vehicleSalesOrder, newOrHistorical).BuildMessage();
            await _extendedLoggingService.LogOutBoundMessage(msg.ToXDocument().ToString(), metadata);

            var soapAddr = new WebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var request = new VolvoSoapRequest(_soapRequestFactory.CreateProcessRequest(soapAddr, msg));

            var soapResult = await _volvoClient.OAuthSendSoapAsync(request, metadata);
            soapResult.ThrowIfNotSuccessful();
            _logger.LogInformationWithMetadata("Finished Processing Used Vehicle Sales Invoice", metadata);
        }

        private async Task<VolvoSettings> GetValidatedVolvoSettingsAsync(Dictionary<string, string> metadata)
        {
            var settings = await _settingsProvider.GetSettingsAsync();
            if (!settings.IsValid(out var errMsg))
            {
                var errMeta = BuildErrorMetadata(metadata, Metrics.InvalidSettings, errMsg);
                _logger.LogInformationWithMetadata("Found Invalid Settings", errMeta);
                throw new InvalidSettingsException(errMsg);
            }
            return settings;
        }

        private Dictionary<string, string> BuildErrorMetadata(Dictionary<string, string> metadata, string metric, string errorMessage) =>
            new Dictionary<string, string>(metadata)
            {
                [TelemetryKeys.Metric] = metric,
                [TelemetryKeys.ErrorMessage] = errorMessage
            };
    }
}