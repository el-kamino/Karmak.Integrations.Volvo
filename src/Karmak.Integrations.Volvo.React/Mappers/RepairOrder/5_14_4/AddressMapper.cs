using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Linq;
using Karmak.Integrations.Volvo.React.Mappers.Shared;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4
{
    public static class AddressMapper
    {
        public static AddressABIEType[] MapFrom(Contact contact)
        {
            return contact.Addresses.Select(address => new AddressABIEType
            {
                ItemsElementName = new[] {
                    AddressElementItemsChoiceTypeStar.LineOne,
                    AddressElementItemsChoiceTypeStar.LineTwo
                },
                Items = new[] {
                    new TextType {
                        Value = address.Street1
                    },
                    string.IsNullOrWhiteSpace(address.Street2)
                        ? null
                        : new TextType {
                            Value = address.Street2
                        }
                },
                CityName = new TextType
                {
                    Value = address.City
                },
                CountryID = string.IsNullOrEmpty(CountryID.ToAlpha3(address.CountryCode))
                    ? "USA"
                    : CountryID.ToAlpha3(address.CountryCode),
                Postcode = string.IsNullOrWhiteSpace(address.PostalCode)
                    ? null
                    : new CodeType
                    {
                        Value = address.PostalCode
                    },
                StateOrProvinceCountrySubDivisionID = string.IsNullOrWhiteSpace(address.Region)
                    ? null
                    : new IdentifierType
                    {
                        Value = address.Region
                    },
                CountyCountrySubDivision = string.IsNullOrWhiteSpace(address.County)
                    ? null
                    : new TextType
                    {
                        Value = address.County
                    }
            }).ToArray();
        }
    }
}