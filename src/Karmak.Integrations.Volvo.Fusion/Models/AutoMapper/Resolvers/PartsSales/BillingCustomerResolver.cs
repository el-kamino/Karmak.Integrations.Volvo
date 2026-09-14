using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.CustomMappers.PartsSales;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.Helpers;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsSales
{
    public class BillingCustomerResolver : IValueResolver<FusionPartsSalesOrder, PartsSalesOrder, ElkCustomer>
    {
        private const string INDIVIDUAL = "Individual";
        public ElkCustomer Resolve(FusionPartsSalesOrder source, PartsSalesOrder destination, ElkCustomer destMember, ResolutionContext context)
        {
            if (source?.BillingCustomer == null)
            {
                return null;
            }

            var sourceCustomer = source.BillingCustomer;

            var destinationCustomer = new ElkCustomer {
                BusinessStructure = sourceCustomer.BusinessStructure,
                CompanyName = sourceCustomer.CompanyName,
                CustomerKey = sourceCustomer.CustomerKey,
                Contacts = MapContacts(sourceCustomer, context.Mapper),
                CustomerTypeCode = new CustomerTypeCodeResolver().Resolve(sourceCustomer, null, null, null)
            };

            destinationCustomer.ExternalIdentifiers = new CustomerIdentifierResolver().Resolve(sourceCustomer, destinationCustomer, destinationCustomer.ExternalIdentifiers, context);

            var primaryContact = destinationCustomer.Contacts.First();
            primaryContact.Addresses = MapCustomerAddresses(sourceCustomer, context.Mapper);
            primaryContact.Email = MapEmail(sourceCustomer);
            primaryContact.Phones = MapPhones(sourceCustomer, primaryContact);

            return destinationCustomer;
        }

        private static List<Contact> MapContacts(FusionCustomer source, IRuntimeMapper mapper)
        {
            var contacts = new List<Contact>();
            mapper.Map(source.Contacts, contacts);
            if (contacts.Count == 0)
            {
                contacts.Add(new Contact());
            }
            return contacts;
        }

        private static List<Address> MapCustomerAddresses(FusionCustomer customer, IRuntimeMapper mapper)
        {
            var mappedAddresses = new List<Address>();

            if (customer.Addresses != null && customer.Addresses.Any()) {
                mapper.Map(customer.Addresses, mappedAddresses);
            }

            return mappedAddresses;
        }

        private static string MapEmail(FusionCustomer customer)
        {
            return BusinessStructure.IsIndividual(customer)
                ? customer.Contacts.FirstOrDefault()?.Email
                : customer.PartsInvoiceEmailAddress;
        }

        private static IList<Phone> MapPhones(FusionCustomer sourceCustomer, Contact primaryContact) {
            return BusinessStructure.IsOrganization(sourceCustomer) ? CustomerPhoneMapper.Map(sourceCustomer) : primaryContact.Phones;
        }
    }
}