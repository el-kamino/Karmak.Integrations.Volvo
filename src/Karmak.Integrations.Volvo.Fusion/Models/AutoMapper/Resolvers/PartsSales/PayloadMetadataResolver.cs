using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsSales
{
    public class PayloadMetadataResolver : IValueResolver<FusionPartsSalesOrder, PartsSalesOrder, PayloadMetadata> {
        public PayloadMetadata Resolve(
            FusionPartsSalesOrder source,
            PartsSalesOrder destination,
            PayloadMetadata destMember,
            ResolutionContext context) {
            return new PayloadMetadata { FusionVersion = source.DatabaseVersion };
        }
    }
}
