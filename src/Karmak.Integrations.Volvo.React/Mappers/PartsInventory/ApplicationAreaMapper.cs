using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Settings;
using Karmak.Integrations.Volvo.React.Utils;
using System;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public static class ApplicationAreaMapper
    {
        private const int MAX_FUSION_VERSION_LENGTH = 9;
        private const string CREATOR_NAME_CODE = "Karmak";
        private const string SENDER_NAME_CODE = "KM";
        private const string VENDOR_ID = "KMKF01";
        private const string DAILY = "D";
        private const string HISTORICAL = "H";

        public static ApplicationAreaTypeStar Map(VolvoSettings settings, PartsInventoryReport partsInventoryReport)
        {
            return new ApplicationAreaTypeStar
            {
                Sender = new SenderTypeStar
                {
                    TaskID = new IdentifierType
                    {
                        Value = partsInventoryReport.Type == ReportType.Full ? HISTORICAL : DAILY
                    },
                    CreatorNameCode = new TextType
                    {
                        Value = CREATOR_NAME_CODE
                    },
                    SenderNameCode = new CodeType
                    {
                        Value = SENDER_NAME_CODE
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
                    SystemVersion = partsInventoryReport.Metadata.FusionVersion.MaxLength(MAX_FUSION_VERSION_LENGTH),
                    PartyID = new IdentifierType
                    {
                        Value = VENDOR_ID
                    }
                },
                CreationDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(
                    DateTime.UtcNow.ToLocalTime(),
                    partsInventoryReport.TimeZone),
                BODID = new IdentifierType
                {
                    Value = Guid.NewGuid().ToString()
                },
                Destination = new DestinationType()
            };
        }

    }
}