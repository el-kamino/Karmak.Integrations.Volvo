using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.PartsSalesOrders.Extensions;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Validators.PartsSaleOrders;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class OrganizationMapper
    {
        private const string PRIMARY_CONTACT_SCHEME_ID = "dealerID";
        private const int COMPANY_NAME_MAX_LENGTH = 80;
        private const int CUSTOMER_ID_MAX_LENGTH = 10;
        private const int EMAIL_MAX_LENGTH = 100;
        private const string EMAIL_CODE = "email1";

        public static OrganizationABIEType MapShipToParty(Customer customer) {
            return Map(customer, customer.GetShippingAddress());
        }

        public static OrganizationABIEType MapBillToParty(Customer customer) {
            return Map(customer, customer.GetBillingAddress());
        }

        private static OrganizationABIEType Map(Customer customer, Address address)
        {
            return new OrganizationABIEType
            {
                CompanyName = new NameTypeOagisUnqualified
                {
                    Value = customer.CompanyName.MaxLength(COMPANY_NAME_MAX_LENGTH)
                },
                PrimaryContact = MapPrimaryContact(customer),
                PostalAddress = AddressValidator.IsValid(address)
                    ? new [] { AddressMapper.Map(address) }
                    : null
            };
        }

        private static ContactABIETypeStar[] MapPrimaryContact(Customer customer)
        {
            if(customer == null)
            {
                return null;
            }

            var contact = customer.Contacts.FirstOrDefault();
            return new[] {
                new ContactABIETypeStar {
                    ID = new[] {
                        new IdentifierType {
                            schemeID = PRIMARY_CONTACT_SCHEME_ID,
                            Value = customer.CustomerKey.MaxLength(CUSTOMER_ID_MAX_LENGTH)
                        }
                    },
                    TypeCode = new CodeType {
                        Value = "Company"
                    },
                    TelephoneCommunication = contact?.Phones.Map(PhoneType.WORK, PhoneType.CELL),
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
                    }
                }
            };
        }
    }
}