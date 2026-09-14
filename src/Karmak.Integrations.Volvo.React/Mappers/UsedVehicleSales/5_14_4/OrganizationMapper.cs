using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using System.Linq;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class OrganizationMapper
    {
        private const int MAX_COMPANY_NAME_LENGTH = 80;

        public static OrganizationABIEType Map(Customer customer)
        {
            var contact = customer.Contacts?.FirstOrDefault();

            return new OrganizationABIEType
            {
                CompanyName = new NameTypeOagisUnqualified
                {
                    Value = customer.CompanyName.MaxLength(MAX_COMPANY_NAME_LENGTH)
                },
                PrimaryContact = ContactMapper.MapPrimary(contact, customer),
                PostalAddress = AddressMapper.Map(contact.GetBillToAddress())
            };
        }
    }
}