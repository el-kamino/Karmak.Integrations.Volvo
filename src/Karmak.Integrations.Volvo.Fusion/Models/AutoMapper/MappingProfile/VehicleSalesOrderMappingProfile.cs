using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.VehicleSales;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.VehicleSalesOrder;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile
{
    public class VehicleSalesOrderMappingProfile : Profile
    {
        public VehicleSalesOrderMappingProfile()
        {
            CreateMap<FusionVehicleSalesOrder, VehicleSalesOrder>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom<PayloadMetadataResolver>())
                .ForMember(dest => dest.Customer, opt => opt.MapFrom<BillingCustomerResolver>())
                .ForMember(dest => dest.SoldVehicles, opt => opt.MapFrom(src => src.VehiclesSold))
                .ForMember(dest => dest.TradeInVehicles, opt => opt.MapFrom(src => src.VehiclesTraded))
                .ForMember(dest => dest.SalesPersonId, opt => opt.MapFrom(src => src.SalesPerson))
                .ForMember(dest => dest.SalesDate, opt => opt.MapFrom(src => src.InvoiceDateTime))
                .ForMember(dest => dest.DealerInfo, opt => opt.Ignore())
                .ForMember(dest => dest.ForceTransmission, opt => opt.Ignore())
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom<TimeZoneResolver<FusionVehicleSalesOrder, VehicleSalesOrder>>());
            CreateMap<FusionCustomer, ElkCustomer>()
                .ForMember(dest => dest.ExternalIdentifiers, opt => opt.MapFrom<CustomerIdentifierResolver>())
                .ForMember(dest => dest.Phones, opt => opt.MapFrom<CustomerPhonesResolver>());
            CreateMap<FusionAddress, Address>();
            CreateMap<FusionContact, Contact>()
                .ForMember(dest => dest.Phones, opt => opt.MapFrom<ContactPhoneResolver>());
            CreateMap<FusionPayloadMetadata, PayloadMetadata>()
                .ForMember(dest => dest.CreatedAtDateTime, opt => opt.MapFrom(src => src.CreatedAtDateTime))
                .ForMember(dest => dest.FusionVersion, opt => opt.MapFrom(src => src.DatabaseVersion));
            CreateMap<FusionVehicle, Vehicle>()
                .ForMember(dest => dest.ExternalIdentifiers, opt => opt.MapFrom<VehicleIdentifierResolver>())
                .ForMember(dest => dest.Condition, opt => opt.MapFrom<VehicleConditionResolver>())
                .ForMember(dest => dest.Odometer, opt => opt.MapFrom<OdometerResolver>())
                .ForMember(dest => dest.UnitInventoryIdentifier, opt => opt.MapFrom(src => src.UnitInventoryID));
            CreateMap<FusionEngine, Engine>()
                .ForMember(dest => dest.Hours, opt => opt.MapFrom(src => src.TotalEngineHours));
        }
    }
}