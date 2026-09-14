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
    public class ShipToCustomerResolver : IValueResolver<FusionPartsSalesOrder, PartsSalesOrder, ElkCustomer>
    {
        private const string INDIVIDUAL = "Individual";

        public ElkCustomer Resolve(FusionPartsSalesOrder source, PartsSalesOrder destination, ElkCustomer destMember, ResolutionContext context)
        {
            if (source?.ShippingCustomer == null)
            {
                return null;
            }

            var sourceCustomer = source.ShippingCustomer;

            var destinationCustomer = new ElkCustomer
            {
                BusinessStructure = sourceCustomer.BusinessStructure,
                CompanyName = sourceCustomer.CompanyName,
                CustomerKey = sourceCustomer.CustomerKey,
                Contacts = MapContacts(sourceCustomer, context.Mapper)
            };
            destinationCustomer.ExternalIdentifiers = new CustomerIdentifierResolver().Resolve(source.BillingCustomer, destinationCustomer, destinationCustomer.ExternalIdentifiers, context);


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

        private static List<Address> MapCustomerAddresses(FusionPartsSalesOrder partsSaleOrder, IRuntimeMapper mapper) {
            var customerAddresses = partsSaleOrder.ShippingCustomer.Addresses;
            var mappedAddresses = mapper.Map(customerAddresses, new List<Address>());

            if (ShipToAddressOverridden(partsSaleOrder)) {
                var oldShipTo = mappedAddresses.FirstOrDefault(a => a.AddressType.Equals(AddressType.SHIP_TO)) ?? new Address();
                mapper.Map(partsSaleOrder.Addresses.First(), oldShipTo);
            }

            return mappedAddresses;
        }

        private static string MapEmail(FusionCustomer customer)
        {
            return BusinessStructure.IsIndividual(customer)
                ? customer.Contacts.FirstOrDefault()?.Email
                : customer.PartsInvoiceEmailAddress;
        }

        private static IList<Phone> MapPhones(FusionCustomer customer, Contact primaryContact) {
            return BusinessStructure.IsOrganization(customer) ? CustomerPhoneMapper.Map(customer) : primaryContact.Phones;
        }

        private static bool ShipToAddressOverridden(FusionPartsSalesOrder order) {
            return order.Addresses != null && order.Addresses.Any();
        }
    }
}