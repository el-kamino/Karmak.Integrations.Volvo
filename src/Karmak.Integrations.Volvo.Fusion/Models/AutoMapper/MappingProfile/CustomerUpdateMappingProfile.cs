using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.CustomerUpdate;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.CustomerUpdate;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;

public class CustomerUpdateMappingProfile : Profile
{
    public CustomerUpdateMappingProfile()
    {
        CreateMap<FusionCustomerUpdate, CustomerUpdate>()
            .ForMember(dest => dest.DealerInfo, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom<PayloadMetadataResolver>())
            .ForMember(dest => dest.Customer, opt => opt.MapFrom<CustomerResolver>())
            .ForMember(dest => dest.ForceTransmission, opt => opt.Ignore())
            .ForMember(dest => dest.VolvoPassRewardID, opt => opt.MapFrom(src => src.VolvoPassRewardID))
            .ForMember(dest => dest.SentToVolvo, opt => opt.MapFrom(src => src.SentToVolvo))
            .ForMember(dest => dest.AddDate, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentVINs, opt => opt.MapFrom(src => src.CurrentVINs))
            .ForMember(dest => dest.AddedVIN, opt => opt.MapFrom(src => src.AddedVIN))
            .ForMember(dest => dest.RemovedVIN, opt => opt.MapFrom(src => src.RemovedVIN))
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom<TimeZoneResolver<FusionCustomerUpdate, CustomerUpdate>>());
        CreateMap<FusionContact, Contact>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom<ContactPhonesResolver>());
        CreateMap<FusionAddress, Address>();
    }
}
