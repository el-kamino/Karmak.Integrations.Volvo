using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class SoldByPartyMapper
    {
        private const string SOLD_BY_PARTY_ID_TYPE = "local";
        private const int PERSON_ID_MAX_LENGTH = 10;
        public static PartyABIEType Map(PartsSalesOrder partsSalesOrder)
        {
            return new PartyABIEType
            {
                Item = new PersonTypeStar
                {
                    ID = new[] {
                        new IdentifierType {
                            Value = partsSalesOrder.PartPersonID.MaxLength(PERSON_ID_MAX_LENGTH),
                            schemeID = SOLD_BY_PARTY_ID_TYPE
                        }
                    }
                }
            };
        }
    }
}