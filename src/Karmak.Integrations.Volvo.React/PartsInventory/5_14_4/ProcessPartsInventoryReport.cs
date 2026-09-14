using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Extensions;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Messages;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Mappers.PartsInventory;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Splitting;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.PartsInventory;
using Karmak.Integrations.Volvo.React.Validators.Settings;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Karmak.Integrations.Volvo.React.Transport.Soap.SoapResponseExtensions;
using ApplicationAreaMapper = Karmak.Integrations.Volvo.React.Mappers.PartsInventory.ApplicationAreaMapper;

namespace Karmak.Integrations.Volvo.React.Core.PartsInventory.V5_14_4
{
    internal class ProcessPartsInventoryReport : IConsumer<PartsInventoryReportReceived>
    {
        private const string RELEASE_ID = "5.10.2";

        private readonly IVolvoExtendedLoggingService _extendedLoggingService;
        private readonly ISettingsProvider _settingsProvider;
        private readonly ILogger _logger;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly IVolvoClient _volvoClient;
        private readonly IVolvoRequestSplitter _requestSplitter;
        private readonly IKarmakBlobClient _claimCheckClient;
        private readonly ProcessorOptions _options;

        public ProcessPartsInventoryReport(
            IVolvoExtendedLoggingService extendedLoggingService,
            ISettingsProvider settingsProvider,
            ILogger<ProcessPartsInventoryReport> logger,
            ISoapRequestFactory soapRequestFactory,
            IVolvoClient volvoClient,
            IVolvoRequestSplitter requestSplitter,
            [FromKeyedServices("ClaimCheck")] IKarmakBlobClient claimCheckClient,
            IOptions<ProcessorOptions> options)
        {
            _extendedLoggingService = extendedLoggingService;
            _settingsProvider = settingsProvider;
            _logger = logger;
            _soapRequestFactory = soapRequestFactory;
            _volvoClient = volvoClient;
            _requestSplitter = requestSplitter;
            _claimCheckClient = claimCheckClient;
            _options = options.Value;
        }

        public async Task Consume(ConsumeContext<PartsInventoryReportReceived> context)
        {
            await Execute(context.Message.ReportClaimCheck);
        }

        private async Task Execute(PartsInventoryReportClaimCheck inventoryClaimCheck)
        {
            VolvoSettings settings = await GetVolvoSettings(inventoryClaimCheck);
            PartsInventoryReport inventoryReport = await GetPartsInventoryReport(inventoryClaimCheck);

            if (PartsInventoryReportIsValid(inventoryReport))
            {
                await SendPartsInventoryReportToVolvo(settings, inventoryReport);
            }
        }

        private async Task<VolvoSettings> GetVolvoSettings(PartsInventoryReportClaimCheck inventoryClaimCheck)
        {
            VolvoSettings settings = await _settingsProvider.GetSettingsAsync();
            if (!settings.IsValid(out var errorMessage))
            {
                _logger.LogInformationWithMetadata("Found Invalid Settings", new Dictionary<string, string>
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidSettings,
                    [TelemetryKeys.ClaimCheckUri] = inventoryClaimCheck.BlobName?.ToString(),
                    [TelemetryKeys.ErrorMessage] = errorMessage
                });
                throw new InvalidSettingsException(errorMessage);
            }
            return settings;
        }

        private async Task<PartsInventoryReport> GetPartsInventoryReport(PartsInventoryReportClaimCheck inventoryClaimCheck)
        {
            PartsInventoryReport report = await _claimCheckClient.RetrieveAsync<PartsInventoryReport>(inventoryClaimCheck.BlobName);
            await _extendedLoggingService.LogInboundDto(report, report.EntityMetadata());
            return report;
        }

        private bool PartsInventoryReportIsValid(PartsInventoryReport report)
        {
            if (!report.HasParts())
            {
                _logger.LogInformationWithMetadata("Inventory Report contained no parts. No transmission required. Done Processing", report.EntityMetadata());
                return false;
            }
            ValidatePartsInventoryReport(report);
            return true;
        }

        private void ValidatePartsInventoryReport(PartsInventoryReport report)
        {
            if (!report.IsValid(out string errorMessage))
            {
                var metadata = new Dictionary<string, string>(report.EntityMetadata())
                {
                    [TelemetryKeys.Metric] = Metrics.InvalidEntity,
                    [TelemetryKeys.ErrorMessage] = errorMessage
                };
                throw new InvalidEntityException(typeof(PartsInventoryReport), errorMessage, metadata);
            }
        }

        private async Task SendPartsInventoryReportToVolvo(VolvoSettings settings, PartsInventoryReport inventoryReport)
        {
            SoapEnvelope[] requests = await BuildRequestsFromReport(settings, inventoryReport);
            foreach (var request in requests)
            {
                await SendRequestToVolvo(request, inventoryReport);
            }

            _logger.LogInformationWithMetadata("Finished Processing Parts Inventory Report", inventoryReport.EntityMetadata());
        }

        private async Task<SoapEnvelope[]> BuildRequestsFromReport(VolvoSettings settings, PartsInventoryReport inventoryReport)
        {
            var address = new ProcessPartsInventoryWebServiceAddress(settings.RegionSettings.CountryCode + settings.InterfaceOptions.PaCode);
            var payload = BuildMessage(settings, inventoryReport);
            var fullSizeRequest = _soapRequestFactory.CreateProcessRequest(address, payload);
            SoapEnvelope[] requests = _requestSplitter.Split(fullSizeRequest).ToArray();

            await _extendedLoggingService.LogOutBoundMessages(requests.Select(r => r.ToXDocument().ToString()).ToArray(), inventoryReport.EntityMetadata());

            return requests;
        }

        private ProcessPartsInventoryType BuildMessage(VolvoSettings settings, PartsInventoryReport inventoryReport)
        {
            return new ProcessPartsInventoryType
            {
                releaseID = RELEASE_ID,
                systemEnvironmentCode = _options.Environment,
                languageCode = LanguageEnumeratedType.enUS,
                ApplicationArea = ApplicationAreaMapper.Map(settings, inventoryReport),
                ProcessPartsInventoryDataArea = new ProcessPartsInventoryDataAreaType
                {
                    Process = ProcessTypeMapper.Map(),
                    PartsInventory = new[]
                    {
                        new PartsInventoryType
                        {
                            PartsInventoryHeader = InventoryHeaderMapper.Map(inventoryReport.TimeZone),
                            PartsInventoryLine = Mappers.PartsInventory.V5_14_4.InventoryLineMapper.Map(inventoryReport, settings.RegionSettings.CurrencyCode)
                        }
                    }
                }
            };
        }

        private async Task SendRequestToVolvo(SoapEnvelope soapMsg, PartsInventoryReport inventoryReport)
        {
            VolvoSoapRequest request = new VolvoSoapRequest(soapMsg);
            IDictionary<string, string> metadata = new Dictionary<string, string>(inventoryReport.EntityMetadata())
            {
                [TelemetryKeys.ReactInterface] = EntityTypes.PartsInventory,
            };
            var soapResult = await _volvoClient.OAuthSendSoapAsync(request, metadata);
            soapResult.ThrowIfNotSuccessful();
        }
    }
}