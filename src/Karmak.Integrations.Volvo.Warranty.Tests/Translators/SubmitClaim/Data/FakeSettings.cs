using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators.SubmitClaim.Data
{
    public static class FakeSettings
    {
        public static readonly VolvoSettings Instance = new VolvoSettings
        {
            DealerServiceProviderSettings = new DealerServiceProviderSettings
            {
                Code = "Karmak",
                ShortCode = "KM",
                SoftwareName = "Fusion",
                SoftwareVersion = "1.0.0"
            },
            RegionSettings = new RegionSettings
            {
                LanguageCode = "en-US",
                CountryCode = "US",
                CurrencyCode = "USD"
            },
            InterfaceOptions = new InterfaceOptions
            {
                PaCode = "K1234",
                OemUserMappings = new Dictionary<string, string>
                {
                    ["tnugent"] = "010203"
                }
            }
        };
    }
}
