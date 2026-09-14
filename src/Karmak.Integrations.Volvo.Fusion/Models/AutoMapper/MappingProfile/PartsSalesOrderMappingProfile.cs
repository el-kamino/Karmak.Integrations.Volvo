using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsSales;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile
{
    public class PartsSalesOrderMappingProfile : Profile {
        public PartsSalesOrderMappingProfile() {
            CreateMap<FusionPartsSalesOrder, PartsSalesOrder>()
                .ForMember(dest => dest.DealerInfo, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.SaleTypeDescription, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom<PayloadMetadataResolver>())
                .ForMember(dest => dest.CustomerPurchaseOrderNumber, opt => opt.MapFrom(src => src.PoNumber))
                .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.InvoiceDateTime))
                .ForMember(dest => dest.SalesPersonId, opt => opt.MapFrom(src => src.SalesPerson))
                .ForMember(dest => dest.TaxTotal, opt => opt.MapFrom(src => src.SalesTaxTotal))
                .ForMember(dest => dest.BillingCustomer, opt => opt.MapFrom<BillingCustomerResolver>())
                .ForMember(dest => dest.ShipToCustomer, opt => opt.MapFrom<ShipToCustomerResolver>())
                .ForMember(dest => dest.PartPersonID, opt => opt.MapFrom(src => src.UpdateUserName))
                .ForMember(dest => dest.ForceTransmission, opt => opt.Ignore())
                .ForMember(dest => dest.OriginalInvoiceNumber, opt => opt.MapFrom(src => src.OriginalInvoiceNumber))
                .ForMember(dest => dest.OriginalInvoiceDate, opt => opt.MapFrom(src => src.OriginalInvoiceDateTime))
                .ForMember(dest => dest.AddDate, opt => opt.MapFrom(src => src.AddDateTime))
                .ForMember(dest => dest.PartsOrderNumber, opt => opt.MapFrom(src => src.SalesOrderNumber))
                .ForMember(dest => dest.OriginalPartsOrderNumber, opt => opt.MapFrom(src => src.OriginalSalesOrderNumber))
                .ForMember(dest => dest.SalesOrderStatus, opt => opt.Ignore())
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom<TimeZoneResolver<FusionPartsSalesOrder, PartsSalesOrder>>());
            CreateMap<FusionPart, Part>()
                .IncludeMembers(s => s.Part)
                .ForMember(dest => dest.PartCost, opt => opt.MapFrom(src => src.UnitCost))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom<PartPriceResolver>())
                .ForMember(dest => dest.AddDate, opt => opt.MapFrom(src => src.AddDateTime));
            CreateMap<FusionAssemblyPart, AssemblyPart>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.PartCost, opt => opt.MapFrom(src => src.UnitCost))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitListPrice))
                .ForMember(dest => dest.PartType, opt => opt.MapFrom(src => src.PartType));
            CreateMap<FusionAssemblyMiscCharge, AssemblyMiscCharge>();
            CreateMap<FusionPartInventory, Part>()
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.Quantity, opt => opt.Ignore())
                .ForMember(dest => dest.PartCost, opt => opt.Ignore())
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.PartNumber))
                .ForMember(dest => dest.AssemblyMiscCharges, opt => opt.MapFrom(src => src.AssemblyMiscCharges))
                .ForMember(dest => dest.AddDate, opt => opt.Ignore());
            CreateMap<FusionMiscCharge, MiscCharge>()
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.MiscellaneousChargeID))
                .ForMember(dest => dest.AddDate, opt => opt.MapFrom(src => src.AddDateTime));
            CreateMap<FusionContact, Contact>()
                .ForMember(dest => dest.Phones, opt => opt.MapFrom<ContactPhonesResolver>());
            CreateMap<FusionAddress, Address>();
        }
    }
}
