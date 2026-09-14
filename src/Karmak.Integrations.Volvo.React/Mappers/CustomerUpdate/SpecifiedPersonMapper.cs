using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.CustomerUpdate;
using System.Linq;


namespace Karmak.Integrations.Volvo.React.Mappers.CustomerUpdate
{
    public static class SpecifiedPersonMapper
    {
        private const string SPECIFIED_PERSON_SCHEME_ID = "dmsID";
        private const int CUSTOMER_ID_MAX_LENGTH = 30;
        private const string EMAIL_CODE = "email1";
        private const int GIVEN_NAME_MAX_LENGTH = 30;
        private const int MIDDLE_NAME_MAX_LENGTH = 1;
        private const int LAST_NAME_MAX_LENGTH = 80;
        private const int MAX_TITLE_LENGTH = 30;
        private const int SALUTATION_MAX_LENGTH = 30;
        private const int EMAIL_MAX_LENGTH = 100;

        public static PersonTypeStar Map(Customer customer, string languageCode)
        {
            var address = customer.GetBillingAddress();
            var contact = customer.Contacts.FirstOrDefault();
            return new PersonTypeStar
            {
                LanguageCode = new[] {
                    languageCode
                },
                ID = string.IsNullOrWhiteSpace(customer.CustomerKey)
                ? null
                : new[] {
                    new IdentifierType {
                        Value = customer.CustomerKey.Trim().MaxLength(CUSTOMER_ID_MAX_LENGTH),
                        schemeID = SPECIFIED_PERSON_SCHEME_ID
                    }
                },
                GivenName = string.IsNullOrWhiteSpace(contact?.FirstName)
                ? null
                : new[] {
                    new NameTypeOagisUnqualified {
                        Value = contact.FirstName.MaxLength(GIVEN_NAME_MAX_LENGTH)
                    }
                },
                MiddleName = string.IsNullOrWhiteSpace(contact?.MiddleName)
                ? null
                : new NameTypeOagisUnqualified
                {
                    Value = contact.MiddleName.MaxLength(MIDDLE_NAME_MAX_LENGTH)
                },
                FamilyName = new[] {
                    new NameTypeOagisUnqualified {
                        Value = contact?.LastName.MaxLength(LAST_NAME_MAX_LENGTH)
                    }
                },
                Title = string.IsNullOrWhiteSpace(contact.Title)
                    ? null
                    : new TextType
                    {
                        Value = contact.Title.MaxLength(MAX_TITLE_LENGTH)
                    },
                Salutation = string.IsNullOrWhiteSpace(contact?.Salutation)
                ? null
                : new TextType
                {
                    Value = contact.Salutation.MaxLength(SALUTATION_MAX_LENGTH)
                },
                TelephoneCommunication = (contact?.Phones).Map(),
                URICommunication = string.IsNullOrWhiteSpace(contact?.Email)
                ? null
                : new[] {
                    new CommunicationABIETypeStar {
                        URIID = new IdentifierType {
                            Value = contact.Email.MaxLength(EMAIL_MAX_LENGTH)
                        },
                        ChannelCode = new CodeType {
                            Value = EMAIL_CODE
                        }
                    }
                },
                PostalAddress = AddressValidator.IsValid(address)
                    ? new[] { AddressMapper.Map(address) }
                    : null
            };
        }
    }
}