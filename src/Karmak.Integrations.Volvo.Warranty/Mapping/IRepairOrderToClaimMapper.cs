using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    public interface IRepairOrderToClaimMapper
    {
        Claim Map(RepairOrderSnapshot source, VolvoSettings settings, string customerRegion);
    }
}
