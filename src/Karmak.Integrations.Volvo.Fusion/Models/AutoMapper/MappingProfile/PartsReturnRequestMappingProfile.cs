using AutoMapper;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsReturn;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsReturn;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;

public class PartsReturnRequestMappingProfile : Profile
{
    public PartsReturnRequestMappingProfile()
    {
        CreateMap<PartPurchaseOrder, PartsReturnRequest>()
            .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src => src.CreatedDateTime))
            .ForMember(dest => dest.CreatedTimeZone, opt => opt.MapFrom(src => src.CreatedTimeZone))
            .ForMember(dest => dest.PACode, opt => opt.Ignore())
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.PONumber))
            .ForMember(dest => dest.VendorId, opt => opt.Ignore())
            .ForMember(dest => dest.DistributionCode, opt => opt.Ignore())
            .ForMember(dest => dest.PartsToReturn, opt => opt.MapFrom<PartsToReturnResolver>())
            .ForMember(dest => dest.KarmakAccountNumber, opt => opt.MapFrom<KarmakAccountNumberResolver>());
    }
}
