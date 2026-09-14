using AutoMapper;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.InterfaceOptions;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.InterfaceOptions;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;

public class VolvoInterfaceOptionsMappingProfile : Profile
{
    public VolvoInterfaceOptionsMappingProfile()
    {
        CreateMap<FusionVolvoInterfaceOptions, InterfaceOptions>()
            .ForMember(dest => dest.OemUserMappings, opt => opt.MapFrom<OemUserMappingResolver>());
    }
}