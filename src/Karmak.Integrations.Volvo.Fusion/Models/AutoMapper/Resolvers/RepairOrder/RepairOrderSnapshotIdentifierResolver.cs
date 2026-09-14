using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.RepairOrder;

public class RepairOrderSnapshotIdentifierResolver : IValueResolver<FusionRepairOrderSnapshot, RepairOrderSnapshot, IList<ExternalIdentifier>>
{
    public IList<ExternalIdentifier> Resolve(FusionRepairOrderSnapshot source, RepairOrderSnapshot destination, IList<ExternalIdentifier> destMember, ResolutionContext context)
    {
        return new List<ExternalIdentifier>()
        {
            new ExternalIdentifier {
                ID = source.RepairOrderID,
                ExternalSourceType = "FUSION"
            }
        };
    }
}
