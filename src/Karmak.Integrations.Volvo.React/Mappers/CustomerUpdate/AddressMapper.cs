using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.Mappers.CustomerUpdate
{
    public static class AddressMapper
    {
        private const string DEFAULT_COUNTRY_CODE = "USA";
        private const int LINE_MAX_LENGTH = 80;
        private const int CITY_MAX_LENGTH = 40;
        private const int POST_CODE_MAX_LENGTH = 20;
        private const int STATE_OR_PROVINCE_COUNTRY_SUB_DIVISION_ID_MAX_LENGTH = 30;
        private const int COUNTY_COUNTRY_SUB_DIVISION_MAX_LENGTH = 40;

        public static AddressABIEType[] Map(IEnumerable<Address> addresses) {
            return addresses?.Select(Map).ToArray();
        }

        public static AddressABIEType Map(Address address) {
            return new AddressABIEType {
                ItemsElementName = new[] {
                    AddressElementItemsChoiceTypeStar.LineOne,
                    AddressElementItemsChoiceTypeStar.LineTwo
                },
                Items = new[] {
                    string.IsNullOrWhiteSpace(address.Street1)
                        ? null
                        : new TextType {
                            Value = address.Street1.MaxLength(LINE_MAX_LENGTH)
                        },
                    string.IsNullOrWhiteSpace(address.Street2)
                        ? null
                        : new TextType {
                            Value = address.Street2.MaxLength(LINE_MAX_LENGTH)
                        }
                },
                CityName = new TextType {
                    Value = address.City.MaxLength(CITY_MAX_LENGTH)
                },
                CountryID = CountryID.ToAlpha3(address.CountryCode) ?? DEFAULT_COUNTRY_CODE,
                Postcode = string.IsNullOrWhiteSpace(address.PostalCode)
                    ? null
                    : new CodeType {
                        Value = address.PostalCode.MaxLength(POST_CODE_MAX_LENGTH)
                    },
                StateOrProvinceCountrySubDivisionID = string.IsNullOrWhiteSpace(address.Region)
                    ? null
                    : new IdentifierType {
                        Value = address.Region.MaxLength(STATE_OR_PROVINCE_COUNTRY_SUB_DIVISION_ID_MAX_LENGTH)
                    },
                CountyCountrySubDivision = string.IsNullOrWhiteSpace(address.County)
                    ? null
                    : new TextType {
                        Value = address.County.MaxLength(COUNTY_COUNTRY_SUB_DIVISION_MAX_LENGTH)
                    }
            };
        }
    }
}