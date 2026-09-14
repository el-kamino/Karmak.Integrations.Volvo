using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.Common.Settings;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4
{
    public static class ContactMapper
    {
        private const string GOVERNMENT_ID = "governmentID";

        public static PersonTypeStar MapFrom(Contact contact, RegionSettings region)
        {
            return new PersonTypeStar
            {
                ID = string.IsNullOrWhiteSpace(contact.LicenseNumber)
                    ? null
                    : new[] {
                        new IdentifierType {
                            schemeID = GOVERNMENT_ID,
                            Value = contact.LicenseNumber
                        }
                    },
                GivenName = string.IsNullOrWhiteSpace(contact.FirstName)
                    ? null
                    : new[] {
                        new NameTypeOagisUnqualified {
                            Value = contact.FirstName
                        }
                    },
                MiddleName = string.IsNullOrWhiteSpace(contact.MiddleName)
                    ? null
                    : new NameTypeOagisUnqualified
                    {
                        Value = contact.MiddleName
                    },
                FamilyName = new[] {
                    new NameTypeOagisUnqualified {
                        Value = contact.LastName
                    }
                },
                Salutation = string.IsNullOrWhiteSpace(contact.Salutation)
                    ? null
                    : new TextType
                    {
                        Value = contact.Salutation
                    },
                TelephoneCommunication = TelephoneMapper.GetTelephonesFor(contact),
                LanguageCode = new[] {
                    region.LanguageCode
                },
                URICommunication = !string.IsNullOrWhiteSpace(contact.Email)
                    ? new[] {
                        new CommunicationABIETypeStar {
                            URIID = new IdentifierType {
                                Value = contact.Email
                            },
                            ChannelCode = new CodeType {
                                Value = "email1"
                            }
                        }
                    }
                    : null,
                PostalAddress = contact.Addresses != null
                    ? AddressMapper.MapFrom(contact)
                    : null
            };
        }

    }
}