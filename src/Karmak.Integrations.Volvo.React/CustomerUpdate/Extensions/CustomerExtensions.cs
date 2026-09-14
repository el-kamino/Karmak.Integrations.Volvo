using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions
{
    public static class CustomerExtensions {
        public static bool IsIndividual(this Customer customer) => customer.BusinessStructure == "Individual";
        public static bool IsOrganization(this Customer customer) => !customer.IsIndividual();

        public static Address GetShippingAddress(this Customer customer) {
            return customer
                .Contacts
                .FirstOrDefault()
                .GetShipToAddress()
                ?? customer.GetBillingAddress();
        }

        public static Address GetBillingAddress(this Customer customer) {
            return customer
                .Contacts
                .FirstOrDefault()
                .GetBillToAddress();
        }
    }
}