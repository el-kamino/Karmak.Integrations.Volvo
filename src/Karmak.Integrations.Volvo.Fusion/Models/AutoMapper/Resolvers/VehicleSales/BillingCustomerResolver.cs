using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder;
using Karmak.Integrations.Volvo.Fusion.Models.Helpers;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.VehicleSales
{
    public class BillingCustomerResolver : IValueResolver<FusionVehicleSalesOrder, VehicleSalesOrder, ElkCustomer> {
        public ElkCustomer Resolve(FusionVehicleSalesOrder source, VehicleSalesOrder destination, ElkCustomer destMember, ResolutionContext context) {
            if (source.BillingCustomer == null) {
                return null;
            }

            var contact = BusinessStructure.IsIndividual(source.BillingCustomer)
                ? MapContactForIndividual(source.BillingCustomer, context)
                : MapContactForOrganization(source.BillingCustomer, context);

            var customer = new ElkCustomer {
                BusinessStructure = source.BillingCustomer.BusinessStructure,
                CustomerKey = source.BillingCustomer.CustomerKey,
                CompanyName = source.BillingCustomer.CompanyName,
                Contacts = new List<Contact> { contact }
            };
            customer.ExternalIdentifiers = new CustomerIdentifierResolver().Resolve(source.BillingCustomer, customer, customer.ExternalIdentifiers, context);

            return customer;
        }

        private static Contact MapContactForIndividual(FusionCustomer source, ResolutionContext context) {
            var contact = context.Mapper.Map(source.Contacts.FirstOrDefault() ?? new FusionContact(), new Contact());
            context.Mapper.Map(source.Addresses, contact.Addresses);
            return contact;
        }

        private static Contact MapContactForOrganization(FusionCustomer source, ResolutionContext context) {
            var contact = new Contact();
            contact.Phones = new CustomerPhoneResolver().Resolve(source, contact, contact.Phones, context);
            context.Mapper.Map(source.Addresses, contact.Addresses);
            contact.Email = source.SalesInvoiceEmailAddress;
            return contact;
        }
    }
}
