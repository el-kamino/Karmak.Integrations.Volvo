using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.StandardCodes;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Services
{
    public class StandardCodesService : IStandardCodesService
    {
        private readonly ISettingsProvider _volvoSettingsClient;
        private readonly ILogger<StandardCodesService> _logger;
        private readonly IStandardCodesClient _standardCodesClient;

        public StandardCodesService(ISettingsProvider volvoSettingsClient, ILogger<StandardCodesService> logger, IStandardCodesClient standardCodesClient)
        {
            _volvoSettingsClient = volvoSettingsClient;
            _logger = logger;
            _standardCodesClient = standardCodesClient;
        }

        public async Task<IEnumerable<StandardCode>> FetchCodesAsync()
        {
            var volvoSettings = await _volvoSettingsClient.GetSettingsAsync();
            _logger.LogInformationWithMetadata("Finished fetching Volvo settings.", new Dictionary<string, string>
            {
                ["volvoSettings.InterfaceOptions.PACode"] = volvoSettings.InterfaceOptions.PaCode,
                ["volvoSettings.RegionSettings.CountryCode"] = volvoSettings.RegionSettings.CountryCode,
                ["volvoSettings.RegionSettings.LanguageCode"] = volvoSettings.RegionSettings.LanguageCode
            });

            return await _standardCodesClient.FetchCodesAsync(new StandardCodesSettings
            {
                PACode = volvoSettings.InterfaceOptions.PaCode,
                CountryCode = volvoSettings.RegionSettings.CountryCode,
                LanguageCode = volvoSettings.RegionSettings.LanguageCode
            });
        }
    }
}
