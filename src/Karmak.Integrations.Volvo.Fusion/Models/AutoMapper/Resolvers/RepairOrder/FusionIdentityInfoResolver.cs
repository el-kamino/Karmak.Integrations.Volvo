using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.RepairOrder;

public class FusionIdentityInfoResolver : IValueResolver<FusionRepairOrderSnapshot, RepairOrderSnapshot, FusionIdentityInfo>
{
    public FusionIdentityInfo Resolve(FusionRepairOrderSnapshot source, RepairOrderSnapshot destination, FusionIdentityInfo destMember, ResolutionContext context)
    {
        var fusionIdentity = ExtractFusionIdentity(context, Constants.FusionIdentityKey);

        return new FusionIdentityInfo
        {
            AccountCode = fusionIdentity.AccountCode,
            BranchId = fusionIdentity.BranchId,
            BranchCode = fusionIdentity.BranchCode,
            UserId = fusionIdentity.UserId,
            Username = fusionIdentity.Username
        };
    }

    private static FusionIdentity ExtractFusionIdentity(ResolutionContext context, string key) =>
        context.Items[key] as FusionIdentity ?? new FusionIdentity();
}
