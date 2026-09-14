using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;
using Karmak.Integrations.Volvo.React.Validators.CustomerUpdate;

namespace Karmak.Integrations.Volvo.React.Mappers.CustomerUpdate
{
    public static class OrganizationMapper
    {
        private const string ORGANIZATION_SCHEME_ID = "dmsID";
        private const int CUSTOMER_KEY_MAX_LENGTH = 10;
        private const int COMPANY_NAME_MAX_LENGTH = 80;

        public static OrganizationABIEType Map(Customer customer, string langCode)
        {
            Address address = customer.GetBillingAddress();
            return new OrganizationABIEType
            {
                OrganizationID = string.IsNullOrWhiteSpace(customer.CustomerKey)
                ? null
                : new IdentifierType
                {
                    Value = customer.CustomerKey.Trim().MaxLength(CUSTOMER_KEY_MAX_LENGTH),
                    schemeID = ORGANIZATION_SCHEME_ID
                },
                CompanyName = new NameTypeOagisUnqualified
                {
                    Value = customer.CompanyName.MaxLength(COMPANY_NAME_MAX_LENGTH)
                },
                PrimaryContact = MapPrimaryContact(customer, langCode),
                PostalAddress = AddressValidator.IsValid(address)
                    ? new[] { AddressMapper.Map(address) }
                    : null
            };
        }

        private static ContactABIETypeStar[] MapPrimaryContact(Customer customer, string langCode)
        {
            if (customer == null)
            {
                return null;
            }

            return new[] {
                new ContactABIETypeStar {
                    TypeCode = new CodeType
                    {
                        Value = "N/A"
                    },
                    Item = SpecifiedPersonMapper.Map(customer, langCode)
                }
            };
        }
    }
}
