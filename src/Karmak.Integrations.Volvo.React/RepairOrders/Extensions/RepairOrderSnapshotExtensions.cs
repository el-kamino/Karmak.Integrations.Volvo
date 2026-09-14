using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.RepairOrders.Extensions
{
    public static class RepairOrderSnapshotExtensions {
        private const string ShipTo = "Ship-To";
        private const string RepairOrderOwning = "Repair Order Owning";

        public static Address GetOwningCustomerAddress(this RepairOrderSnapshot repairOrder) {
            return repairOrder.Addresses.FirstOrDefault(address => ShipTo.EqualsIgnoreCase(address.AddressType) && RepairOrderOwning.EqualsIgnoreCase(address.EntityType));
        }

        /// <summary>
        /// Checks if this RepairOrder has a reference to an OriginalRepairOrder
        /// </summary>
        public static bool IsSecondaryRepairOrder(this RepairOrderSnapshot repairOrder)
        {
            return !string.IsNullOrEmpty(repairOrder.OriginalRepairOrderNumber);
        }
    }
}