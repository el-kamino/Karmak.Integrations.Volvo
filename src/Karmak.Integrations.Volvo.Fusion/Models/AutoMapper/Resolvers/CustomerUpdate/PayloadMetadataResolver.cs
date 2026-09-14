using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.CustomerUpdate;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.CustomerUpdate;

public class PayloadMetadataResolver : IValueResolver<FusionCustomerUpdate, Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate, PayloadMetadata>
{
    public PayloadMetadata Resolve(
        FusionCustomerUpdate source,
        Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate destination,
        PayloadMetadata destMember,
        ResolutionContext context)
    {
        return new PayloadMetadata { FusionVersion = source.DatabaseVersion };
    }
}
