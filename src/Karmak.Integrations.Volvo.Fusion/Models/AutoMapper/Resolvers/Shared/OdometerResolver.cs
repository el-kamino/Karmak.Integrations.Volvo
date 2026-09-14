using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    public class OdometerResolver : IValueResolver<FusionVehicle, Vehicle, Odometer> {
        public Odometer Resolve(FusionVehicle source, Vehicle destination, Odometer destMember, ResolutionContext context) {
            var unitType = OdometerUnitType.UNKNOWN;

            if (Enum.TryParse<OdometerUnitType>(source?.CurrentMeterType?.ToUpper(), out var meterType)) {
                unitType = meterType;
            }

            var meterReading = source?.CurrentMeterReading ?? 1m;

            return new Odometer {
                Reading = meterReading,
                UnitType = unitType
            };
        }
    }
}
