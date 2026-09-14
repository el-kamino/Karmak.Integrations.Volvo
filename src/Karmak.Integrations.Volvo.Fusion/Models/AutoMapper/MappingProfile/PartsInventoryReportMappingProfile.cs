using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.BeforeActions;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsInventory;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsInventory;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile
{
    public class PartsInventoryReportMappingProfile : Profile
    {
        public PartsInventoryReportMappingProfile()
        {
            CreateMap<FusionPartsInventoryReport, PartsInventoryReport>()
                .BeforeMap<BeforeMappingPartsInventorySnapshot>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom<PayloadMetadataResolver>())
                .ForMember(dest => dest.Type, opt => opt.MapFrom<ReportTypeResolver>())
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom<TimeZoneResolver<FusionPartsInventoryReport, PartsInventoryReport>>());

            CreateMap<FusionPart, InventoryPart>()
                .ForMember(dest => dest.PartNumber, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.LastSoldDate, opt => opt.MapFrom(src => src.LastSold))
                .ForMember(dest => dest.ExtendedDealerCost, opt => opt.MapFrom(src => src.UnitCost))
                .ForMember(dest => dest.QuantityBestStockingLevel, opt => opt.MapFrom(src => src.StockingLevel))
                .ForMember(dest => dest.StockingStatus, opt => opt.MapFrom<StockingStatusResolver>())
                .ForMember(dest => dest.DataForCurrentPeriod, opt => opt.MapFrom<DataForCurrentPeriodResolver>())
                .ForMember(dest => dest.QuantitySoldHistory, opt => opt.MapFrom<QuantitySoldHistoryResolver>())
                .ForMember(dest => dest.IsRimManaged, opt => opt.MapFrom(src => src.IsVolvoRIMManaged))
                .ForMember(dest => dest.IsVolvoPart, opt => opt.MapFrom(src => src.IsVolvoPart))
                .ForMember(dest => dest.TriggerReasonCode, opt => opt.MapFrom(src => src.TriggerReasonCode));
        }
    }
}
