using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared
{
    class TimeZoneResolver<T_Source, T_Dest> : IValueResolver<T_Source, T_Dest, decimal?> {
        public decimal? Resolve(T_Source source, T_Dest destination, decimal? destMember, ResolutionContext context) {
            return context.Items[Constants.TimeZone] as decimal?;
        }
    }
}
