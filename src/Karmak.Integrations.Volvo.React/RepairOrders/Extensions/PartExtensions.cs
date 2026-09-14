using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.React.RepairOrders.Extensions
{
    public static class PartExtensions {
        public static bool IsAssemblyPart(this Part part) {
            return part.AssemblyParts.Any();
        }

        public static bool IsExchangePart(this Part part) {
            return part.PartType == PartType.EXCHANGE || string.IsNullOrWhiteSpace(part.PartType) && !string.IsNullOrWhiteSpace(part.CorePartNumber);
        }

        public static bool IsCorePart(this Part part, IEnumerable<Part> exchangeParts = null) {
            return part.PartType == PartType.CORE || exchangeParts != null && exchangeParts.Any(exchange => exchange.CorePartNumber == part.PartNumber);
        }
    }
}