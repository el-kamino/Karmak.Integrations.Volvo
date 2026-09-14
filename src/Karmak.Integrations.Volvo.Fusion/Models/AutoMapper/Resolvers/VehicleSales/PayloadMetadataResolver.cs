using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.VehicleSales
{
    public class PayloadMetadataResolver : IValueResolver<FusionVehicleSalesOrder, VehicleSalesOrder, PayloadMetadata>
    {
        public PayloadMetadata Resolve(
            FusionVehicleSalesOrder source,
            VehicleSalesOrder destination,
            PayloadMetadata destMember,
            ResolutionContext context)
        {
            return new PayloadMetadata { FusionVersion = source.DatabaseVersion };
        }
    }
}