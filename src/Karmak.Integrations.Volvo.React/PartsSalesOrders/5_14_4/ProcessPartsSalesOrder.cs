using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Extensions;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Karmak.Integrations.Volvo.React.Transport.Soap.SoapResponseExtensions;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    internal class ProcessPartsSalesOrder : IProcessPartsSalesOrder
    {
        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ILogger _logger;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly IVolvoClient _volvoClient;
        private readonly IOptions<ProcessorOptions> _options;

        public ProcessPartsSalesOrder(
            IVolvoExtendedLoggingService extendedLoggingService,
            ISettingsProvider settingsProvider,
            ILogger<ProcessPartsSalesOrder> logger,
            ISoapRequestFactory soapRequestFactory,
            IVolvoClient volvoClient,
            IOptions<ProcessorOptions> options)
        {
            _extendedLoggingService = extendedLoggingService;
            _settingsProvider = settingsProvider;
            _logger = logger;
            _soapRequestFactory = soapRequestFactory;
            _volvoClient = volvoClient;
            _options = options;
        }

        public async Task Execute(PartsSalesOrder partsSalesOrder, string newOrHistorical)
        {
            await _extendedLoggingService.LogInboundDto(partsSalesOrder, partsSalesOrder.EntityMetadata());

            var IsValidPartsSalesOrder = partsSalesOrder.IsValid(out var errorMessage);
            if (!IsValidPartsSalesOrder)
            {
                throw new InvalidEntityException(typeof(PartsSalesOrder), errorMessage, new Dictionary<string, string>(partsSalesOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidEntity,
                    [TelemetryKeys.ErrorMessage] = errorMessage
                });
            }

            var settings = await _settingsProvider.GetSettingsAsync();
            var isValidSettings = settings.IsValid(out errorMessage);
            if (!isValidSettings)
            {
                _logger.LogInformationWithMetadata("Found Invalid Settings UDB5.2", new Dictionary<string, string>(partsSalesOrder.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidSettings,
                    [TelemetryKeys.ErrorMessage] = errorMessage
                });
                throw new InvalidSettingsException(errorMessage);
            }

            var partsInvoiceBod = new CreateProcessPartsInvoice(_options, settings, partsSalesOrder, newOrHistorical).BuildMessage();

            await _extendedLoggingService.LogOutBoundMessage(partsInvoiceBod.ToXDocument().ToString(), partsSalesOrder.EntityMetadata());

            var soapMessageAddress = new ProcessPartsInvoiceWebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var request = _soapRequestFactory.CreateProcessRequest(soapMessageAddress, partsInvoiceBod);
            var requestEnvelope = new VolvoSoapRequest(request);

            var metaData = new Dictionary<string, string>(partsSalesOrder.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.PartSalesOrder,
            };

            (await _volvoClient.OAuthSendSoapAsync(requestEnvelope, metaData)).ThrowIfNotSuccessful();

            _logger.LogInformationWithMetadata($"Finished Processing Parts Sales Order {partsSalesOrder.PartsOrderNumber} UDB5.2", metaData);
        }
    }
}
