using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.CustomMappers.PartsSales;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.CustomerUpdate;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.Helpers;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.CustomerUpdate
{
    public class CustomerResolver : IValueResolver<FusionCustomerUpdate, Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate, ElkCustomer>
    {
        private const string INDIVIDUAL = "Individual";

        public ElkCustomer Resolve(FusionCustomerUpdate source, Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate destination, ElkCustomer destMember, ResolutionContext context)
        {
            if (source?.Customer == null)
                return null;

            var sourceCustomer = source.Customer;

            var destinationCustomer = new ElkCustomer
            {
                BusinessStructure = sourceCustomer.BusinessStructure,
                CompanyName = sourceCustomer.CompanyName,
                CustomerKey = sourceCustomer.CustomerKey,
                Contacts = MapContacts(sourceCustomer, context.Mapper)
            };

            destinationCustomer.ExternalIdentifiers = new CustomerIdentifierResolver().Resolve(source.Customer, destinationCustomer, destinationCustomer.ExternalIdentifiers, context);

            var primaryContact = destinationCustomer.Contacts.First();
            primaryContact.Addresses = MapCustomerAddresses(source, context.Mapper);
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

        private static List<Address> MapCustomerAddresses(FusionCustomerUpdate customerUpdate, IRuntimeMapper mapper)
        {
            var customerAddresses = customerUpdate.Customer.Addresses;
            var mappedAddresses = mapper.Map(customerAddresses, new List<Address>());

            return mappedAddresses;
        }

        private static string MapEmail(FusionCustomer customer)
        {
            return BusinessStructure.IsIndividual(customer)
                ? customer.Contacts.FirstOrDefault()?.Email
                : customer.PartsInvoiceEmailAddress;
        }

        private static IList<Phone> MapPhones(FusionCustomer customer, Contact primaryContact)
        {
            return BusinessStructure.IsOrganization(customer) ? CustomerPhoneMapper.Map(customer) : primaryContact.Phones;
        }
    }
}