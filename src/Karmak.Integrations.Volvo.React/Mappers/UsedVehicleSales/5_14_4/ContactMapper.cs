using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Common.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.UsedVehicleSales.Extensions;
using Karmak.Integrations.Volvo.React.Utils;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class ContactMapper
    {
        private const string DMS_SCHEME_ID = "dmsID";
        private const string DEALER_SCHEME_ID = "dealerID";
        private const string SYSTEM_SCHEME_ID = "systemID";
        private const string REQUIRED_CONTACT_TYPE_CODE = "Company";
        private const int MAX_DMS_CUSTOMER_IDENTIFIER_LENGTH = 30;
        private const int MAX_SYSTEM_CUSTOMER_IDENTIFIER_LENGTH = 100;
        private const int MAX_DEALER_PRIMARY_CONTACT_IDENTIFIER_LENGTH = 10;
        private const int MAX_SYSTEM_PRIMARY_CONTACT_IDENTIFIER_LENGTH = 100;
        private const int MAX_EMAIL_LENGTH = 100;
        private const int MAX_GIVEN_NAME_LENGTH = 30;
        private const int MAX_MIDDLE_NAME_LENGTH = 1;
        private const int MAX_FAMILY_NAME_LENGTH = 80;
        private const int MAX_TITLE_LENGTH = 30;
        private const int MAX_SALUTATION_LENGTH = 30;

        public static ContactABIETypeStar[] MapPrimary(Contact contact, Customer customer)
        {
            if (contact == null)
            {
                return null;
            }

            return new[] {
                new ContactABIETypeStar {
                    TypeCode = new CodeType {
                        Value = REQUIRED_CONTACT_TYPE_CODE
                    },
                    ID = GetOrganizationIdList(customer),
                    TelephoneCommunication = TelephoneMapper.GetTelephonesFor(contact?.Phones, PhoneType.WORK, PhoneType.CELL),
                    URICommunication = string.IsNullOrWhiteSpace(contact.Email)
                    ? null
                    : new[] {
                        new CommunicationABIETypeStar {
                            ChannelCode = new CodeType {
                                Value = ChannelCodes.EmailChannelCode,
                            },
                            URIID = new IdentifierType {
                                Value = contact.Email.MaxLength(MAX_EMAIL_LENGTH)
                            }
                        }
                    }
                }
            };
        }

        public static PersonTypeStar Map(Contact contact, Customer customer, string languageCode)
        {
            return new PersonTypeStar
            {
                LanguageCode = new[] {
                    languageCode
                },
                ID = GetIndividualIdList(customer),
                GivenName = string.IsNullOrWhiteSpace(contact.FirstName)
                    ? null
                    : new[] {
                    new NameTypeOagisUnqualified {
                        Value = contact.FirstName.MaxLength(MAX_GIVEN_NAME_LENGTH)
                    }
                },
                MiddleName = string.IsNullOrWhiteSpace(contact.MiddleName)
                    ? null
                    : new NameTypeOagisUnqualified
                    {
                        Value = contact.MiddleName.MaxLength(MAX_MIDDLE_NAME_LENGTH)
                    },
                FamilyName = new[] {
                    new NameTypeOagisUnqualified {
                        Value = contact.LastName.MaxLength(MAX_FAMILY_NAME_LENGTH)
                    }
                },
                Title = string.IsNullOrWhiteSpace(contact.Title)
                    ? null
                    : new TextType
                    {
                        Value = contact.Title.MaxLength(MAX_TITLE_LENGTH)
                    },
                Salutation = string.IsNullOrWhiteSpace(contact.Salutation)
                    ? null
                    : new TextType
                    {
                        Value = contact.Salutation.MaxLength(MAX_SALUTATION_LENGTH)
                    },
                TelephoneCommunication = TelephoneMapper.GetTelephonesFor(contact.Phones),
                URICommunication = string.IsNullOrWhiteSpace(contact.Email)
                    ? null
                    : new[] {
                    new CommunicationABIETypeStar {
                        URIID = new IdentifierType {
                            Value = contact.Email.MaxLength(MAX_EMAIL_LENGTH)
                        },
                        ChannelCode = new CodeType {
                            Value = ChannelCodes.EmailChannelCode
                        }
                    }
                },
                PostalAddress = AddressMapper.Map(contact.GetBillToAddress())
            };
        }

        private static IdentifierType[] GetOrganizationIdList(Customer customer)
        {
            return GetIdentifierList(
                customer,
                MAX_DEALER_PRIMARY_CONTACT_IDENTIFIER_LENGTH,
                DEALER_SCHEME_ID,
                MAX_SYSTEM_PRIMARY_CONTACT_IDENTIFIER_LENGTH,
                SYSTEM_SCHEME_ID
            );
        }

        private static IdentifierType[] GetIndividualIdList(Customer customer)
        {
            return GetIdentifierList(
                customer,
                MAX_DMS_CUSTOMER_IDENTIFIER_LENGTH,
                DMS_SCHEME_ID,
                MAX_SYSTEM_CUSTOMER_IDENTIFIER_LENGTH,
                SYSTEM_SCHEME_ID
            );
        }

        private static IdentifierType CreateIdentifier(string value, int maxLength, string schemeID)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : new IdentifierType
                {
                    Value = value.MaxLength(maxLength),
                    schemeID = schemeID
                };
        }

        private static IdentifierType[] GetIdentifierList(Customer customer, int customerKeyMaxLength, string customerKeySchemeID, int fusionIdMaxLength, string fusionIdSchemeID)
        {
            var list = new[]
            {
                CreateIdentifier(customer.CustomerKey, customerKeyMaxLength, customerKeySchemeID),
                CreateIdentifier(customer.GetFusionId(), fusionIdMaxLength, fusionIdSchemeID)
            }.Where(id => id != null).ToList();

            return list.Any() ? list.ToArray() : null;
        }
    }
}