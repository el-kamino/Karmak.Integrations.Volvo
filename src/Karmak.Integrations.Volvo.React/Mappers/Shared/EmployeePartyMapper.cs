using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.Shared
{
    public static class EmployeePartyMapper
    {
        private const string ID_TYPE = "local";
        private const int MAX_ID_LENGTH = 10;

        public static PartyABIEType[] Map(string id)
        {
            return string.IsNullOrWhiteSpace(id)
                ? null
                : new[] {
                    new PartyABIEType {
                        Item = new PersonTypeStar {
                        ID = new[] {
                            new IdentifierType {
                                schemeID = ID_TYPE,
                                Value = id.MaxLength(MAX_ID_LENGTH)
                            }
                        }
                    }
                }
            };
        }
    }
}