using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using System;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public static class InventoryHeaderMapper
    {
        private const string DEFAULT_DOCUMENT_ID = "1";
        public static PartsInventoryHeaderType Map(decimal? tz)
        {
            return new PartsInventoryHeaderType
            {
                DocumentDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(DateTime.UtcNow.ToLocalTime(), tz),
                DocumentDateTimeSpecified = true,
                DocumentIdentificationGroup = new DocumentIdentificationGroupType
                {
                    DocumentIdentification = new DocumentIdentificationType
                    {
                        DocumentID = new IdentifierType
                        {
                            Value = DEFAULT_DOCUMENT_ID
                        }
                    }
                }
            };
        }
    }
}