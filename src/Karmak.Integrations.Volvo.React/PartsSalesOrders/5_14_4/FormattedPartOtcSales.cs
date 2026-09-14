using Karmak.Integrations.Volvo.React.Core.Common.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    public class FormattedPartNumberOtcSales : FormattedPartNumber
    {
        public FormattedPartNumberOtcSales(string partNumber) : base(partNumber) { }

        public override ItemIdentificationType[] ToItemIdentificationGroup()
        {
            var partNumbers = new List<ItemIdentificationType> {
                new ItemIdentificationType {
                    ItemID = new IdentifierType {
                        schemeID = PART_PREFIX.ToLower(),
                        Value = Prefix
                    }
                }
            };

            if (HasBase)
            {
                partNumbers.Add(new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeID = PART_BASE.ToLower(),
                        Value = Base
                    }
                });
            }

            if (HasSuffix)
            {
                partNumbers.Add(new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeID = PART_SUFFIX.ToLower(),
                        Value = Suffix
                    }
                });
            }

            return partNumbers.ToArray();
        }
    }
}