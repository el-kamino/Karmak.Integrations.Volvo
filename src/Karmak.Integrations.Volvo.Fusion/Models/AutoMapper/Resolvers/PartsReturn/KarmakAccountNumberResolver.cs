using AutoMapper;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsReturn;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsReturn;

public class KarmakAccountNumberResolver : IValueResolver<PartPurchaseOrder, PartsReturnRequest, string>
{
    private const string FusionIdentityKey = "FusionIdentity";

    public string Resolve(PartPurchaseOrder source, PartsReturnRequest destination, string destMember, ResolutionContext context)
    {
        return ExtractFusionIdentity(context, FusionIdentityKey).AccountCode;
    }

    private static FusionIdentity ExtractFusionIdentity(ResolutionContext context, string key) =>
        context.Items[key] as FusionIdentity ?? new FusionIdentity();
}
