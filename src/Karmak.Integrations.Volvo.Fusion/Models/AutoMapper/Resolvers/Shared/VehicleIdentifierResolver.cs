using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    public class VehicleIdentifierResolver : IValueResolver<FusionVehicle, Vehicle, IList<ExternalIdentifier>> {
        public IList<ExternalIdentifier> Resolve(FusionVehicle source, Vehicle destination, IList<ExternalIdentifier> destMember, ResolutionContext context) {
            return new List<ExternalIdentifier>
            {
                new ExternalIdentifier {
                    ID = source.UnitID,
                    ExternalSourceType = "FUSION"
                }
            };
        }
    }
}
