using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.RepairOrder;

public class TaskTaxTotalResolver : IValueResolver<FusionRepairOrderTask, RepairOrderTask, decimal>
{
    public decimal Resolve(FusionRepairOrderTask source, RepairOrderTask destination, decimal destMember, ResolutionContext context)
    {
        return source.TaskTaxTotal ?? 0m;
    }
}