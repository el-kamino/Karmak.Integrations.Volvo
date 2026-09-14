using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class AddressMapper
    {
        private const string DEFAULT_COUNTRY_CODE = "USA";
        private const int MAX_LINE_LENGTH = 80;
        private const int MAX_CITY_LENGTH = 30;
        private const int MAX_POSTAL_CODE_LENGTH = 20;
        private const int MAX_STATE_LENGTH = 30;
        private const int MAX_COUNTY_LENGTH = 40;

        public static AddressABIEType[] Map(Address address) {
            // from Motive spec: requires city name & country ID to be valid
            // from code: if address.CountryCode fails to map in CountryID.ToAlpha3(), mapper uses a default code, no sense in checking
            // so, valid if city name is set, otherwise false; no need to check the rest
            if (string.IsNullOrWhiteSpace(address?.City))
                return null;

            return new[] {
                new AddressABIEType
                {
                    ItemsElementName = new[] {
                        AddressElementItemsChoiceTypeStar.LineOne,
                        AddressElementItemsChoiceTypeStar.LineTwo
                    },
                    Items = new[] {
                        string.IsNullOrWhiteSpace(address.Street1)
                            ? null
                            : new TextType {
                                Value = address.Street1.MaxLength(MAX_LINE_LENGTH)
                            },
                        string.IsNullOrWhiteSpace(address.Street2)
                            ? null
                            : new TextType {
                                Value = address.Street2.MaxLength(MAX_LINE_LENGTH)
                            }
                    },
                    CityName = string.IsNullOrWhiteSpace(address.City)
                        ? null
                        : new TextType
                        {
                            Value = address.City.MaxLength(MAX_CITY_LENGTH)
                        },
                    CountryID = string.IsNullOrWhiteSpace(address.CountryCode)
                        ? null
                        : CountryID.ToAlpha3(address.CountryCode),
                    Postcode = string.IsNullOrWhiteSpace(address.PostalCode)
                        ? null
                        : new CodeType
                        {
                            Value = address.PostalCode.MaxLength(MAX_POSTAL_CODE_LENGTH)
                        },
                    StateOrProvinceCountrySubDivisionID = string.IsNullOrWhiteSpace(address.Region)
                        ? null
                        : new IdentifierType
                        {
                            Value = address.Region.MaxLength(MAX_STATE_LENGTH)
                        },
                    CountyCountrySubDivision = string.IsNullOrWhiteSpace(address.County)
                        ? null
                        : new TextType
                        {
                            Value = address.County.MaxLength(MAX_COUNTY_LENGTH)
                        }
                }
            };
        }
    }
}