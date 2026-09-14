using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.Settings;
using Karmak.Integrations.Volvo.React.Utils;
using System;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared
{
    public static class ApplicationAreaMapper
    {
        private const int MAX_FUSION_VERSION_LENGTH = 9;

        public static ApplicationAreaTypeStar Map(
            VolvoSettings settings, 
            string fusionVersion, 
            string newOrHistorical, 
            decimal? tz)
        {
            return new ApplicationAreaTypeStar
            {
                Sender = new SenderTypeStar
                {
                    TaskID = new IdentifierType
                    {
                        Value = newOrHistorical
                    },
                    CreatorNameCode = new TextType
                    {
                        Value = DspConstants.Code
                    },
                    SenderNameCode = new CodeType
                    {
                        Value = DspConstants.ShortCode
                    },
                    DealerNumberID = new IdentifierType
                    {
                        Value = settings.InterfaceOptions.PaCode
                    },
                    AreaNumber = new TextType
                    {
                        Value = FranchiseCodeType.Fetch(settings.InterfaceOptions.FranchiseCode)
                    },
                    DealerCountryCode = CountryID.ToEnumeratedCountry(settings.RegionSettings.CountryCode),
                    DealerCountryCodeSpecified = true,
                    LanguageCode = settings.RegionSettings.LanguageCode,
                    SystemVersion = fusionVersion.MaxLength(MAX_FUSION_VERSION_LENGTH),
                    PartyID = new IdentifierType
                    {
                        Value = DspConstants.Identifier
                    }
                },
                CreationDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(DateTime.UtcNow.ToLocalTime(), tz),
                BODID = new IdentifierType
                {
                    Value = Guid.NewGuid().ToString()
                },
                Destination = new DestinationType()
            };
        }
    }
}