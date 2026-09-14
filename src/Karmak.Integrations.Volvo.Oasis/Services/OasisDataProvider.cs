using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Oasis.Configuration;
using Karmak.Integrations.Volvo.Oasis.Exceptions;
using Karmak.Integrations.Volvo.Oasis.Mapping;
using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Oasis.Services
{
    public class OasisDataProvider : IOasisDataProvider
    {
        private const string ApplicationType = "GOASIS";

        private readonly IOasisClient _client;
        private readonly IOasisRequestMapper _mapper;
        private readonly ISettingsProvider _settings;
        private readonly IExtendedLoggingClient _extendedLogging;
        private readonly ILogger _logger;
        private readonly OasisDataProviderOptions _options;

        public OasisDataProvider(
            IOasisClient client,
            IOasisRequestMapper mapper,
            ISettingsProvider settings,
            IExtendedLoggingClient extendedLogging,
            ILogger<OasisDataProvider> logger,
            IOptions<OasisDataProviderOptions> options)
        {
            _client = client;
            _mapper = mapper;
            _settings = settings;
            _extendedLogging = extendedLogging;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<OasisResponseRest> GetDataAsync(OasisRequestRest request)
        {
            _logger.LogInformationWithMetadata(
                "Oasis Data Provider received GetData request",
                new Dictionary<string, string>
                {
                    ["Vin"] = request.Vin,
                    ["IncludeFsaData"] = request.IncludeFsaData.ToString(),
                    ["IncludeBroadcastMessages"] = request.IncludeBroadcastMessages.ToString(),
                    ["IncludeVehicleInfo"] = request.IncludeVehicleInfo.ToString(),
                    ["IncludeWarrantyData"] = request.IncludeWarrantyData.ToString(),
                    ["SymptomCodes"] = BuildSymptomCodesForLogging(request),
                    ["ComplaintCode"] = BuildComplaintCodeForLogging(request)
                });

            var oasisRequest = _mapper.Map(request);

            SetRequestValuesFromConfigs(oasisRequest);
            await SetRequestValuesFromSettings(oasisRequest);

            await _extendedLogging.Execute(oasisRequest.ToXml(), "OASIS REQUEST", new Dictionary<string, string>
            {
                ["OasisUri"] = _options.Uri,
                ["Vin"] = request.Vin,
                ["IncludeBroadcastMessages"] = request.IncludeBroadcastMessages.ToString()
            });

            var response = await _client.SendAsync(_options.Uri, oasisRequest, request.KarmakAccountNumber);

            return response;
        }

        private void SetRequestValuesFromConfigs(OasisRequest oasisRequest)
        {
            oasisRequest.ApplicationId = _options.ApplicationId;
            oasisRequest.ImsRegion = _options.ImsRegion;
            oasisRequest.UserId = _options.UserId;
            oasisRequest.XmlOnly = true;
            oasisRequest.ApplicationType = ApplicationType;
        }

        private async Task SetRequestValuesFromSettings(OasisRequest oasisRequest)
        {
            var settings = await _settings.GetSettingsAsync();

            var paCode = settings.InterfaceOptions.PaCode?.Trim();
            if (string.IsNullOrWhiteSpace(paCode))
                throw new OasisConfigException("PA Code is not set in settings.");

            paCode = paCode.EndsWith("!") ? paCode.Substring(0, paCode.Length - 1) : paCode;

            if (paCode.Length > 5)
                throw new OasisConfigException("PA Code cannot be longer than 5 alpha-numeric characters.");

            oasisRequest.PaCode = paCode;

            if (settings.RegionSettings == null)
                throw new OasisConfigException("Neither Country Code nor Language Code can be determined at this time.  Please set them in settings.");

            if (string.IsNullOrWhiteSpace(settings.RegionSettings.CountryCode))
                throw new OasisConfigException("Country Code cannot be determined at this time.  Please set them in settings.");

            if (string.IsNullOrWhiteSpace(settings.RegionSettings.LanguageCode))
                throw new OasisConfigException("Language Code cannot be determined at this time.  Please set them in settings.");

            oasisRequest.Gsa = settings.RegionSettings.CountryCode;
            oasisRequest.LanguageCode = settings.RegionSettings.LanguageCode;
        }

        private static string BuildSymptomCodesForLogging(OasisRequestRest request)
        {
            if (request.SymptomCodes == null || !request.SymptomCodes.Any())
                return string.Empty;

            return string.Join(", ", request.SymptomCodes.OrderBy(sc => sc.Order).Select(sc => sc.Order));
        }

        private static string BuildComplaintCodeForLogging(OasisRequestRest request)
        {
            if (request.ComplaintCode == null)
                return string.Empty;

            return $"Code: {request.ComplaintCode.Code}, Type: {request.ComplaintCode.CodeType}, Description: {request.ComplaintCode.Description}";
        }
    }
}
