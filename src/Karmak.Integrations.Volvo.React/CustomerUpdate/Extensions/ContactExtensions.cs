using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions
{
    public static class ContactExtensions {
        public static Address GetBillToAddress(this Contact contact) {
            return contact?.Addresses?.FirstOrDefault(address => address.AddressType == AddressTypes.BillTo);
        }

        public static Address GetShipToAddress(this Contact contact) {
            return contact?.Addresses?.FirstOrDefault(address => address.AddressType == AddressTypes.ShipTo);
        }
    }
}