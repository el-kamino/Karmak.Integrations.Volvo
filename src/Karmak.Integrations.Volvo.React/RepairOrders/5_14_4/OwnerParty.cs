using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4
{
    public static class OwnerParty
    {
        private const string INDIVIDUAL = "Individual";
        private const string PERSON_ID_CODE = "dmsID";
        private const string USA = "USA";
        private const string EMAIL_CHANNEL_CODE = "email1";
        private const string PRIVACY_PARTY_USE_CODE = "corporate";
        private const string PRIMARY_CONTACT_TYPE_CODE = "Company";

        private const int STREET_MAX_LENGTH = 80;
        private const int CITY_MAX_LENGTH = 40;
        private const int COMPANY_NAME_MAX_LENGTH = 80;
        private const int GIVEN_NAME_MAX_LENGTH = 30;
        private const int FAMILY_NAME_MAX_LENGTH = 80;

        public static PartyABIEType FetchOrNull(RepairOrderSnapshot repairOrder)
        {
            object item = Fetch(repairOrder);
            if (item == null)
                return null;
            return new PartyABIEType
            {
                Item = item
            };
        }

        public static object Fetch(RepairOrderSnapshot repairOrder)
        {
            if (repairOrder.OwningCustomer.BusinessStructure != null && repairOrder.OwningCustomer.BusinessStructure.EqualsIgnoreCase(INDIVIDUAL))
            {
                return FetchContactPerson(repairOrder);
            }
            return FetchContactOrganization(repairOrder);
        }

        private static PersonTypeStar FetchContactPerson(RepairOrderSnapshot repairOrder)
        {
            // we neutered the validation, so need to manage cases here

            if (string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.Contacts?.FirstOrDefault().LastName))
                return null;

            return new PersonTypeStar
            {
                LanguageCode = new[] {
                    "en-US"
                },
                ID = new[] {
                    new IdentifierType {
                        Value = repairOrder.OwningCustomer.CustomerKey,
                        schemeID = PERSON_ID_CODE
                    }
                },
                GivenName = string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.Contacts?[0].FirstName)
                    ? null
                    : new[] {
                        new NameTypeOagisUnqualified {
                            Value = repairOrder.OwningCustomer.Contacts?[0].FirstName.MaxLength(GIVEN_NAME_MAX_LENGTH)
                        }
                    },
                MiddleName = string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.Contacts?[0].MiddleName)
                    ? null
                    : new NameTypeOagisUnqualified
                    {
                        Value = repairOrder.OwningCustomer.Contacts?[0].MiddleName
                    },
                FamilyName = string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.Contacts?[0].LastName)
                    ? null
                    : new[] {
                        new NameTypeOagisUnqualified {
                            Value = repairOrder.OwningCustomer.Contacts?[0].LastName.MaxLength(FAMILY_NAME_MAX_LENGTH)
                        }
                    },
                Salutation = string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.Contacts?[0].Salutation)
                    ? null
                    : new TextType
                    {
                        Value = repairOrder.OwningCustomer.Contacts?[0].Salutation
                    },
                PostalAddress = new[] {
                    FetchOwningAddress(repairOrder)
                },
                TelephoneCommunication = TelephoneMapper.GetTelephonesFor(repairOrder.OwningCustomer?.Contacts?.First()),
                URICommunication = FetchURICommunication(repairOrder.OwningCustomer.Contacts?[0])
            };
        }

        private static OrganizationABIEType FetchContactOrganization(RepairOrderSnapshot repairOrder)
        {
            if (string.IsNullOrWhiteSpace(repairOrder.OwningCustomer.CompanyName))
                return null;

            return new OrganizationABIEType
            {
                CompanyName = new NameTypeOagisUnqualified
                {
                    Value = repairOrder.OwningCustomer.CompanyName.MaxLength(COMPANY_NAME_MAX_LENGTH)
                },
                PrimaryContact = FetchPrimaryContacts(repairOrder),
                PostalAddress = new[] {
                    FetchOwningAddress(repairOrder)
                }
            };
        }

        private static ContactABIETypeStar[] FetchPrimaryContacts(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.OwningCustomer.Contacts.Select(contact => new ContactABIETypeStar
            {
                ID = new[] {
                    new IdentifierType {
                        Value = repairOrder.OwningCustomer.CustomerKey
                    }
                },
                TelephoneCommunication = TelephoneMapper.GetTelephonesFor(repairOrder.OwningCustomer?.Contacts?.First(), PhoneType.WORK, PhoneType.CELL),
                URICommunication = FetchURICommunication(contact),
                TypeCode = new CodeType
                {
                    Value = PRIMARY_CONTACT_TYPE_CODE
                }
            }).ToArray();
        }

        private static CommunicationABIETypeStar[] FetchURICommunication(Contact contact)
        {
            if (string.IsNullOrWhiteSpace(contact?.Email))
            {
                return null;
            }
            return new[] {
                new CommunicationABIETypeStar {
                    URIID = new IdentifierType {
                        Value = contact.Email
                    },
                    ChannelCode = new CodeType {
                        Value = EMAIL_CHANNEL_CODE
                    },
                    UseCode = new CodeType {
                        Value = PRIVACY_PARTY_USE_CODE
                    }
                }
            };
        }

        private static AddressABIEType FetchOwningAddress(RepairOrderSnapshot repairOrder) {
            var owningAddress = repairOrder.GetOwningCustomerAddress();
            if (owningAddress == null) return null;

            var addressType = new AddressABIEType
            {
                ItemsElementName = new[] {
                        AddressElementItemsChoiceTypeStar.LineOne,
                        AddressElementItemsChoiceTypeStar.LineTwo
                    }
            };
            addressType.Items = GetItems(owningAddress).ToArray();
            addressType.CityName = GetCityName(owningAddress);
            addressType.CountryID = GetCountryId(owningAddress);
            addressType.Postcode = GetPostalCode(owningAddress);
            addressType.StateOrProvinceCountrySubDivisionID = GetStateOrProvinceCountrySubDivisionId(owningAddress);
            addressType.CountyCountrySubDivision = GetCountyCountrySubDivision(owningAddress);

            return addressType;
        }

        private static TextType GetCityName(Address address)
        {
            if (string.IsNullOrWhiteSpace(address?.City))
                return null;
            else
                return new TextType
                {
                    Value = address?.City.MaxLength(CITY_MAX_LENGTH)
                };
        }

        private static string GetCountryId(Address address)
        {
            var code = CountryEvaluator.ToAlpha3(address?.CountryCode);
            if (string.IsNullOrEmpty(code))
                code = USA;
            return code;
        }

        private static CodeType GetPostalCode(Address address)
        {
            if (string.IsNullOrWhiteSpace(address?.PostalCode))
                return null;
            else
                return new CodeType
                {
                    Value = address?.PostalCode
                };
        }

        private static IdentifierType GetStateOrProvinceCountrySubDivisionId(Address address)
        {
            if (string.IsNullOrWhiteSpace(address?.Region))
                return null;
            else
                return new IdentifierType
                {
                    Value = address?.Region
                };
        }

        private static TextType GetCountyCountrySubDivision(Address address)
        {
            if (string.IsNullOrWhiteSpace(address?.County))
                return null;
            else
                return new TextType
                {
                    Value = address?.County
                };
        }

        private static IEnumerable<TextType> GetItems(Address address)
        {
            if (string.IsNullOrWhiteSpace(address?.Street1)) yield return null;
            else
            {
                yield return new TextType
                {
                    Value = address?.Street1.MaxLength(STREET_MAX_LENGTH)
                };
            }

            if (string.IsNullOrWhiteSpace(address?.Street2)) yield return null;
            else
            {
                yield return new TextType
                {
                    Value = address?.Street2.MaxLength(STREET_MAX_LENGTH)
                };
            }
        }
    }
}