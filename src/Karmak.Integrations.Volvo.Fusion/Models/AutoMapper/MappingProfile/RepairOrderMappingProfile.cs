using AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.BeforeActions;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.RepairOrder;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.Shared;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using ElkCustomer = Karmak.Integrations.Volvo.React.Contracts.Common.Customer;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.MappingProfile;

public class RepairOrderMappingProfile : Profile
{
    public RepairOrderMappingProfile()
    {
        CreateMap<FusionRepairOrderSnapshot, RepairOrderSnapshot>()
            .ForPath(dest => dest.SnapshotId, opt => opt.Ignore())
            .BeforeMap<BeforeMappingFusionRepairOrderSnapshot>()
            .ForMember(dest => dest.SnapshotSequenceNumber, opt => opt.Ignore())
            .ForMember(dest => dest.FusionIdentityInfo, opt => opt.MapFrom<FusionIdentityInfoResolver>())
            .ForMember(dest => dest.DealerInfo, opt => opt.Ignore())
            .ForMember(dest => dest.InvoiceIdentifier, opt => opt.MapFrom(src => src.InvoiceNumber))
            .ForMember(dest => dest.SnapshotSequenceNumberDateTime, opt => opt.Ignore())
            .ForMember(dest => dest.ExternalIdentifiers, opt => opt.MapFrom<RepairOrderSnapshotIdentifierResolver>())
            .ForMember(dest => dest.OriginalRepairOrderExternalIdentifiers, opt => opt.MapFrom<RepairOrderSnapshotOriginalIdentifierResolver>())
            .ForMember(dest => dest.ForceTransmission, opt => opt.Ignore())
            .ForMember(dest => dest.TimeZone, opt => opt.MapFrom<TimeZoneResolver<FusionRepairOrderSnapshot, RepairOrderSnapshot>>());
        CreateMap<FusionAddress, Address>();
        CreateMap<FusionAppointment, Appointment>()
            .ForMember(dest => dest.ArrivalDateTime, opt => opt.MapFrom(src => src.ArrivalDate))
            .ForMember(dest => dest.AppointmentMadeDateTime, opt => opt.MapFrom(src => src.AppointmentMadeDate));
        CreateMap<FusionAssemblyPart, AssemblyPart>();
        CreateMap<FusionAssemblyMiscCharge, AssemblyMiscCharge>();
        CreateMap<FusionContact, Contact>()
            .ForMember(dest => dest.Phones, opt => opt.MapFrom<ContactPhonesResolver>());
        CreateMap<FusionCustomer, ElkCustomer>()
            .ForMember(dest => dest.ExternalIdentifiers, opt => opt.MapFrom<CustomerIdentifierResolver>())
            .ForMember(dest => dest.Phones, opt => opt.MapFrom<CustomerPhonesResolver>())
            .ForMember(dest => dest.CustomerTypeCode, opt => opt.MapFrom<CustomerTypeCodeResolver>());
        CreateMap<FusionLabor, Labor>()
            .ForMember(dest => dest.DateTimeIn, opt => opt.MapFrom(src => src.TimeIn))
            .ForMember(dest => dest.DateTimeOut, opt => opt.MapFrom(src => src.TimeOut))
            .ForMember(dest => dest.InvoiceIdentifier, opt => opt.MapFrom(src => src.MiscPONumber))
            .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.MiscPODate))
            .ForMember(dest => dest.TechnicianUsername, opt => opt.MapFrom(src => src.TechnicianUserName))
            .ForMember(dest => dest.TechnicianFullName, opt => opt.MapFrom(src => src.TechnicianName));
        CreateMap<FusionMiscCharge, MiscCharge>()
            .ForMember(dest => dest.InvoiceIdentifier, opt => opt.MapFrom(src => src.MiscPONumber))
            .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.MiscPODate));
        CreateMap<FusionPart, Part>()
            .ForMember(dest => dest.InvoiceIdentifier, opt => opt.MapFrom(src => src.MiscPONumber))
            .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.MiscPODate))
            .ForMember(dest => dest.AssemblyMiscCharges, opt => opt.MapFrom(src => src.AssemblyMiscCharges));
        CreateMap<FusionVehicle, Vehicle>()
            .ForMember(dest => dest.ExternalIdentifiers, opt => opt.MapFrom<VehicleIdentifierResolver>())
            .ForMember(dest => dest.Odometer, opt => opt.MapFrom<OdometerResolver>())
            .ForMember(dest => dest.Condition, opt => opt.MapFrom<VehicleConditionResolver>())
            .ForMember(dest => dest.UnitInventoryIdentifier, opt => opt.MapFrom(src => src.UnitInventoryID));
        CreateMap<FusionEngine, Engine>()
            .ForMember(dest => dest.Hours, opt => opt.MapFrom(src => src.TotalEngineHours));
        CreateMap<FusionComplaintCauseCorrection, RepairOrderTask>()
            .ForPath(dest => dest.LaborOperations, opt => opt.Ignore())
            .ForPath(dest => dest.TaskNumber, opt => opt.Ignore())
            .ForPath(dest => dest.RepairTaskStatus, opt => opt.Ignore())
            .ForPath(dest => dest.RepairTaskSubStatus, opt => opt.Ignore())
            .ForPath(dest => dest.Parts, opt => opt.Ignore())
            .ForPath(dest => dest.MiscCharges, opt => opt.Ignore())
            .ForPath(dest => dest.LaborEntries, opt => opt.Ignore())
            .ForPath(dest => dest.Warranty, opt => opt.Ignore())
            .ForPath(dest => dest.RepairType, opt => opt.Ignore())
            .ForPath(dest => dest.SRTID, opt => opt.Ignore())
            .ForPath(dest => dest.RepairTypeDescription, opt => opt.Ignore())
            .ForPath(dest => dest.RepairTypeId, opt => opt.Ignore())
            .ForPath(dest => dest.TaskTaxTotal, opt => opt.Ignore())
            .ForPath(dest => dest.OverrideLaborRate, opt => opt.Ignore())
            .ForPath(dest => dest.LaborRate, opt => opt.Ignore())
            .ForPath(dest => dest.AlternateBillingCustomerKey, opt => opt.Ignore())
            .ForPath(dest => dest.DepartmentID, opt => opt.Ignore())
            .ForPath(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.CauseDescription, opt => opt.MapFrom(src => src.CauseDescription))
            .ForMember(dest => dest.ComplaintDescription, opt => opt.MapFrom(src => src.ComplaintDescription))
            .ForMember(dest => dest.CorrectionDescription, opt => opt.MapFrom(src => src.CorrectionDescription))
            .ForMember(dest => dest.ComplaintNotes, opt => opt.MapFrom<ComplaintNotesResolver>())
            .ForMember(dest => dest.InternalDealerNotes, opt => opt.MapFrom(src => src.MiscNotes))
            .ForMember(dest => dest.TechnicianNotes, opt => opt.MapFrom(src => src.CorrectionDescription));
        CreateMap<FusionRepairOrderTask, RepairOrderTask>()
            .ForMember(dest => dest.LaborOperations, opt => opt.MapFrom(src => src.SRTs))
            .ForMember(dest => dest.TaskTaxTotal, opt => opt.MapFrom<TaskTaxTotalResolver>())
            .ForMember(dest => dest.LaborRate, opt => opt.MapFrom(src => src.LaborRate))
            .ForMember(dest => dest.OverrideLaborRate, opt => opt.MapFrom(src => src.OverrideLaborRate))
            .IncludeMembers(src => src.ComplaintCauseCorrection);
        CreateMap<FusionSRT, LaborOperation>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.SRTCode))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.SRTDescription))
            .ForMember(dest => dest.Hours, opt => opt.MapFrom(src => src.SRTHours));
    }
}