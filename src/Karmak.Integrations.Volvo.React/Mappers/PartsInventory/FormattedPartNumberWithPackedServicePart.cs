using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Common;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public class FormattedPartNumberWithPackedServicePart : FormattedPartNumber
    {
        private const int PART_NUMBER_MAX_LENGTH = 22;
        private const string PACKED_SERVICE_PART = "Packed Service Part";
        private readonly string _partNumber;

        public FormattedPartNumberWithPackedServicePart(string partNumber) : base(partNumber)
        {
            _partNumber = partNumber.RemoveSpecialCharacters().MaxLength(PART_NUMBER_MAX_LENGTH);
        }

        public override ItemIdentificationType[] ToItemIdentificationGroup()
        {
            return new List<ItemIdentificationType>(base.ToItemIdentificationGroup()) {
                new ItemIdentificationType {
                    ItemID = new IdentifierType {
                        schemeName = PACKED_SERVICE_PART,
                        Value = _partNumber
                    }
                }
            }.ToArray();
        }
    }
}