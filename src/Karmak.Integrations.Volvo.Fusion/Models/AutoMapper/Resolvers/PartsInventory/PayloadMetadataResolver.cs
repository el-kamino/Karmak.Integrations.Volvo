using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;

public class PayloadMetadataResolver : IValueResolver<FusionPartsInventoryReport, PartsInventoryReport, PayloadMetadata>
{
    public PayloadMetadata Resolve(FusionPartsInventoryReport source, PartsInventoryReport destination, PayloadMetadata destMember, ResolutionContext context)
    {
        return new PayloadMetadata
        {
            CreatedAtDateTime = source.PartInventoryMetaData.CreatedDateTime,
            FusionVersion = source.PartInventoryMetaData.DatabaseVersion
        };
    }
}
