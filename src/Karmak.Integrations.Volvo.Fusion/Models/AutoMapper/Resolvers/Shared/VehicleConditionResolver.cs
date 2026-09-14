using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    public class VehicleConditionResolver : IValueResolver<FusionVehicle, Vehicle, VehicleCondition> {
        public VehicleCondition Resolve(FusionVehicle source, Vehicle destination, VehicleCondition destMember, ResolutionContext context) {
            return source.IsNewUnit ? VehicleCondition.New : VehicleCondition.Used;
        }
    }
}
