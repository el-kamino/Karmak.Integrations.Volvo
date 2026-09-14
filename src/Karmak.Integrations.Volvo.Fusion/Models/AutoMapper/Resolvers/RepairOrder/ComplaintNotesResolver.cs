using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.RepairOrder;

public class ComplaintNotesResolver : IValueResolver<FusionComplaintCauseCorrection, RepairOrderTask, string>
{
    public string Resolve(FusionComplaintCauseCorrection source, RepairOrderTask destination, string destMember, ResolutionContext context) =>
        source == null
            ? null
            : string.Concat(source.ComplaintDescription, source.CauseDescription);
}