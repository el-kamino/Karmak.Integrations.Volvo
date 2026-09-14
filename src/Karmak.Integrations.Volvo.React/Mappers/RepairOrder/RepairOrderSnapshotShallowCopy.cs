using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Mappers.RepairOrder
{
    public static class RepairOrderSnapshotShallowCopy
    {
        public static RepairOrderSnapshot GetShallowCopy(RepairOrderSnapshot xro)
        {
            var ro = new RepairOrderSnapshot();
            ro.ROTotal = xro.ROTotal;
            ro.ROTaxAmountTotal = xro.ROTaxAmountTotal;
            ro.RepairOrderStatus = xro.RepairOrderStatus;
            ro.SubStatus = xro.SubStatus;
            ro.PromisedDate = xro.PromisedDate;
            ro.OpenDate = xro.OpenDate;
            ro.CompletionDate = xro.CompletionDate;
            ro.ArrivalDate = xro.ArrivalDate;
            ro.InvoiceDate = xro.InvoiceDate;
            ro.InServiceDate = xro.InServiceDate;
            ro.FusionSchedulingDate = xro.FusionSchedulingDate;
            ro.Department = xro.Department;
            ro.DepartmentID = xro.DepartmentID;
            ro.OriginalRepairOrderNumber = xro.OriginalRepairOrderNumber;
            ro.InvoiceIdentifier = xro.InvoiceIdentifier;
            ro.MeterType = xro.MeterType;
            ro.MeterReading = xro.MeterReading;
            ro.ServiceWriterName = xro.ServiceWriterName;
            ro.CustomerContactedStatus = xro.CustomerContactedStatus;
            ro.SnapshotId = xro.SnapshotId;
            ro.SnapshotSequenceNumber = xro.SnapshotSequenceNumber;
            ro.SnapshotSequenceNumberDateTime = xro.SnapshotSequenceNumberDateTime;
            ro.ForceTransmission = xro.ForceTransmission;
            ro.FusionIdentityInfo = GetShallowCopy(xro.FusionIdentityInfo ?? new FusionIdentityInfo());
            ro.DealerInfo = GetShallowCopy(xro.DealerInfo ?? new DealerInfo());
            ro.Addresses = GetShallowCopy(xro.Addresses ?? new List<Address>());
            ro.ExternalIdentifiers = GetShallowCopy(xro.ExternalIdentifiers ?? new List<ExternalIdentifier>());
            ro.DatabaseVersion = xro.DatabaseVersion;
            ro.BillingCustomer = GetShallowCopy(xro.BillingCustomer ?? new Customer());
            ro.Driver = GetShallowCopy(xro.Driver ?? new Contact());
            ro.Tasks = GetShallowCopy(xro.Tasks ?? new List<RepairOrderTask>());
            ro.RepairOrderNumber = xro.RepairOrderNumber;
            ro.CustomerPONumber = xro.CustomerPONumber;
            ro.Vehicle = GetShallowCopy(xro.Vehicle ?? new Vehicle());
            ro.Appointment = GetShallowCopy(xro.Appointment ?? new Appointment());
            ro.OwningCustomer = GetShallowCopy(xro.OwningCustomer ?? new Customer());
            ro.OriginalRepairOrderExternalIdentifiers = GetShallowCopy(xro.OriginalRepairOrderExternalIdentifiers ?? new List<ExternalIdentifier>());
            ro.TimeZone = xro.TimeZone;

            return ro;
        }

        public static FusionIdentityInfo GetShallowCopy(FusionIdentityInfo xfusionIdentityInfo)
        {
            var fusionIdentityInfo = new FusionIdentityInfo();
            fusionIdentityInfo.AccountCode = xfusionIdentityInfo?.AccountCode;
            fusionIdentityInfo.BranchId = xfusionIdentityInfo?.BranchId;
            fusionIdentityInfo.BranchCode = xfusionIdentityInfo?.BranchCode;
            fusionIdentityInfo.UserId = xfusionIdentityInfo?.UserId;
            fusionIdentityInfo.Username = xfusionIdentityInfo?.Username;
            return fusionIdentityInfo;
        }

        public static DealerInfo GetShallowCopy(DealerInfo xdealerInfo)
        {
            var dealerInfo = new DealerInfo();
            dealerInfo.AccountId = xdealerInfo.AccountId;
            dealerInfo.InstanceId = xdealerInfo.InstanceId;
            dealerInfo.BranchId = xdealerInfo.BranchId;
            return dealerInfo;
        }

        public static IList<Address> GetShallowCopy(IList<Address> xaddresses)
        {
            var addresses = new List<Address>();
            if (xaddresses != null)
                foreach (var address in xaddresses)
                    addresses.Add(GetShallowCopy(address));
            return addresses;
        }

        public static Address GetShallowCopy(Address xaddress)
        {
            var address = new Address();
            address.AddressType = xaddress?.AddressType;
            address.EntityType = xaddress?.EntityType;
            address.Street1 = xaddress?.Street1;
            address.Street2 = xaddress?.Street2;
            address.City = xaddress?.City;
            address.PostalCode = xaddress?.PostalCode;
            address.Region = xaddress?.Region;
            address.County = xaddress?.County;
            address.CountryCode = xaddress?.CountryCode;
            return address;
        }

        public static IList<ExternalIdentifier> GetShallowCopy(IList<ExternalIdentifier> xexternalIdentifiers)
        {
            var externalIdentifiers = new List<ExternalIdentifier>();
            if (xexternalIdentifiers != null)
                foreach (var item in xexternalIdentifiers)
                    externalIdentifiers.Add(GetShallowCopy(item));
            return externalIdentifiers;
        }

        public static ExternalIdentifier GetShallowCopy(ExternalIdentifier xitem)
        {
            var externalIdentifier = new ExternalIdentifier();
            externalIdentifier.ID = xitem?.ID;
            externalIdentifier.ExternalSourceType = xitem?.ExternalSourceType;
            return externalIdentifier;
        }

        public static Customer GetShallowCopy(Customer xcustomer)
        {
            var customer = new Customer();
            customer.CustomerKey = xcustomer?.CustomerKey;
            customer.CompanyName = xcustomer?.CompanyName;
            customer.BusinessStructure = xcustomer?.BusinessStructure;
            customer.CustomerTypeCode = xcustomer?.CustomerTypeCode;
            customer.Contacts = GetShallowCopy(xcustomer?.Contacts);
            customer.Addresses = GetShallowCopy(xcustomer?.Addresses);
            customer.Phones = GetShallowCopy(xcustomer?.Phones);
            customer.ExternalIdentifiers = GetShallowCopy(xcustomer?.ExternalIdentifiers);
            return customer;
        }

        public static IList<Contact> GetShallowCopy(IList<Contact> xcontacts)
        {
            var contacts = new List<Contact>();

            if (xcontacts != null)
                foreach (var item in xcontacts)
                    contacts.Add(GetShallowCopy(item));
            return contacts;
        }

        public static Contact GetShallowCopy(Contact xcontact)
        {
            var contact = new Contact();
            contact.Title = xcontact?.Title;
            contact.Salutation = xcontact?.Salutation;
            contact.FirstName = xcontact?.FirstName;
            contact.MiddleName = xcontact?.MiddleName;
            contact.LastName = xcontact?.LastName;
            contact.Phones = GetShallowCopy(xcontact?.Phones);
            contact.Email = xcontact?.Email;
            contact.Addresses = GetShallowCopy(xcontact?.Addresses);
            contact.LicenseNumber = xcontact?.LicenseNumber;
            return contact;
        }

        public static IList<Phone> GetShallowCopy(IList<Phone> xphones)
        {
            var phones = new List<Phone>();
            if (xphones != null)
                foreach (var item in xphones)
                    phones.Add(GetShallowCopy(item));
            return phones;
        }

        public static Phone GetShallowCopy(Phone xphone)
        {
            var phone = new Phone();
            phone.Type = xphone?.Type ?? PhoneType.HOME;
            phone.Number = xphone?.Number;
            return phone;
        }

        public static IList<RepairOrderTask> GetShallowCopy(IList<RepairOrderTask> xtasks)
        {
            var tasks = new List<RepairOrderTask>();
            if (xtasks != null)
                foreach (var item in xtasks)
                    tasks.Add(GetShallowCopy(item));
            return tasks;
        }

        public static RepairOrderTask GetShallowCopy(RepairOrderTask xtask)
        {
            var task = new RepairOrderTask();
            task.TechnicianNotes = xtask?.TechnicianNotes;
            task.ComplaintNotes = xtask?.ComplaintNotes;
            task.AlternateBillingCustomerKey = xtask?.AlternateBillingCustomerKey;
            task.OverrideLaborRate = xtask?.OverrideLaborRate ?? 0;
            task.LaborRate = xtask?.LaborRate ?? 0;
            task.TaskTaxTotal = xtask?.TaskTaxTotal ?? 0;
            task.RepairTypeId = xtask?.RepairTypeId ?? 0;
            task.RepairTypeDescription = xtask?.RepairTypeDescription;
            task.SRTID = xtask?.SRTID;
            task.RepairType = xtask?.RepairType;
            task.Warranty = xtask?.Warranty;
            task.LaborEntries = GetShallowCopy(xtask?.LaborEntries);
            task.MiscCharges = GetShallowCopy(xtask?.MiscCharges);
            task.Parts = GetShallowCopy(xtask?.Parts);
            task.RepairTaskSubStatus = xtask?.RepairTaskSubStatus;
            task.RepairTaskStatus = xtask?.RepairTaskStatus;
            task.TaskNumber = xtask?.TaskNumber;
            task.InternalDealerNotes = xtask?.InternalDealerNotes;
            task.LaborOperations = GetShallowCopy(xtask?.LaborOperations);
            task.DepartmentID = xtask?.DepartmentID;
            task.Department = xtask?.Department;
            return task;
        }

        public static IList<Labor> GetShallowCopy(IList<Labor> xlaborEntries)
        {
            var laborEntries = new List<Labor>();
            if (xlaborEntries != null)
                foreach (var item in xlaborEntries)
                    laborEntries.Add(GetShallowCopy(item));
            return laborEntries;
        }

        public static Labor GetShallowCopy(Labor xlabor)
        {
            var labor = new Labor();
            labor.UnitPrice = xlabor?.UnitPrice;
            labor.ExtendedPrice = xlabor?.ExtendedPrice;
            labor.TotalHours = xlabor?.TotalHours;
            labor.TechnicianNumber = xlabor?.TechnicianNumber;
            labor.TechnicianUsername = xlabor?.TechnicianUsername;
            labor.TechnicianFullName = xlabor?.TechnicianFullName;
            labor.DateTimeIn = xlabor?.DateTimeIn;
            labor.DateTimeOut = xlabor?.DateTimeOut;
            labor.InvoiceIdentifier = xlabor?.InvoiceIdentifier;
            labor.InvoiceDate = xlabor?.InvoiceDate;
            return labor;
        }

        public static IList<MiscCharge> GetShallowCopy(IList<MiscCharge> xmiscCharges)
        {
            var miscCharges = new List<MiscCharge>();
            if (xmiscCharges != null)
                foreach (var item in xmiscCharges)
                    miscCharges.Add(GetShallowCopy(item));
            return miscCharges;
        }

        public static MiscCharge GetShallowCopy(MiscCharge xmiscCharge)
        {
            var miscCharge = new MiscCharge();
            miscCharge.MiscellaneousChargeID = xmiscCharge?.MiscellaneousChargeID;
            miscCharge.MiscellaneousChargeType = xmiscCharge?.MiscellaneousChargeType;
            miscCharge.IncludeMiscellaneousChargeType = xmiscCharge?.IncludeMiscellaneousChargeType;
            miscCharge.ExtendedPrice = xmiscCharge?.ExtendedPrice;
            miscCharge.Name = xmiscCharge?.Name;
            miscCharge.Description = xmiscCharge?.Description;
            miscCharge.Quantity = xmiscCharge?.Quantity;
            miscCharge.UnitCost = xmiscCharge?.UnitCost;
            miscCharge.UnitPrice = xmiscCharge?.UnitPrice;
            miscCharge.InvoiceIdentifier = xmiscCharge?.InvoiceIdentifier;
            miscCharge.InvoiceDate = xmiscCharge?.InvoiceDate;
            return miscCharge;
        }

        public static IList<Part> GetShallowCopy(IList<Part> xparts)
        {
            var parts = new List<Part>();
            if (xparts != null)
                foreach (var item in xparts)
                    parts.Add(GetShallowCopy(item));
            return parts;
        }

        public static Part GetShallowCopy(Part xpart)
        {
            var part = new Part();
            part.AssemblyMiscCharges = GetShallowCopy(xpart?.AssemblyMiscCharges);
            part.AssemblyParts = GetShallowCopy(xpart?.AssemblyParts);
            part.CorePartNumber = xpart?.CorePartNumber;
            part.PartType = xpart?.PartType;
            part.CoreQuantity = xpart?.CoreQuantity;
            part.CoreExtendedCost = xpart?.CoreExtendedCost;
            part.CoreExtendedPrice = xpart?.CoreExtendedPrice;
            part.CoreUnitPrice = xpart?.CoreUnitPrice;
            part.InvoiceIdentifier = xpart?.InvoiceIdentifier;
            part.CoreUnitCost = xpart?.CoreUnitCost;
            part.UnitPrice = xpart?.UnitPrice;
            part.Quantity = xpart?.Quantity;
            part.ExtendedCost = xpart?.ExtendedCost;
            part.UnitCost = xpart?.UnitCost;
            part.Description = xpart?.Description;
            part.PartNumber = xpart?.PartNumber;
            part.AddDate = xpart?.AddDate;
            part.AddUsername = xpart?.AddUsername;
            part.ExtendedPrice = xpart?.ExtendedPrice;
            part.InvoiceDate = xpart?.InvoiceDate;
            return part;
        }

        public static IList<AssemblyMiscCharge> GetShallowCopy(IList<AssemblyMiscCharge> xassemblyMiscCharges)
        {
            var assemblyMiscCharges = new List<AssemblyMiscCharge>();
            if (xassemblyMiscCharges != null)
                foreach (var item in xassemblyMiscCharges)
                    assemblyMiscCharges.Add(GetShallowCopy(item));
            return assemblyMiscCharges;
        }

        public static AssemblyMiscCharge GetShallowCopy(AssemblyMiscCharge xassemblyMiscCharge)
        {
            var assemblyMiscCharge = new AssemblyMiscCharge();
            assemblyMiscCharge.MiscellaneousChargeID = xassemblyMiscCharge?.MiscellaneousChargeID;
            assemblyMiscCharge.MiscellaneousChargeType = xassemblyMiscCharge?.MiscellaneousChargeType;
            assemblyMiscCharge.Name = xassemblyMiscCharge?.Name;
            assemblyMiscCharge.Description = xassemblyMiscCharge?.Description;
            assemblyMiscCharge.Quantity = xassemblyMiscCharge?.Quantity;
            assemblyMiscCharge.UnitListPrice = xassemblyMiscCharge?.UnitListPrice;
            assemblyMiscCharge.UnitCost = xassemblyMiscCharge?.UnitCost;
            return assemblyMiscCharge;
        }

        public static IList<AssemblyPart> GetShallowCopy(IList<AssemblyPart> xassemblyParts)
        {
            var assemblyParts = new List<AssemblyPart>();
            if (xassemblyParts != null)
                foreach (var item in xassemblyParts)
                    assemblyParts.Add(GetShallowCopy(item));
            return assemblyParts;
        }

        public static AssemblyPart GetShallowCopy(AssemblyPart xassemblyPart)
        {
            var assemblyPart = new AssemblyPart();
            assemblyPart.UnitCost = xassemblyPart?.UnitCost;
            assemblyPart.UnitListPrice = xassemblyPart?.UnitListPrice;
            assemblyPart.Quantity = xassemblyPart?.Quantity;
            assemblyPart.Description = xassemblyPart?.Description;
            assemblyPart.PartNumber = xassemblyPart?.PartNumber;
            assemblyPart.PartType = xassemblyPart?.PartType;
            assemblyPart.CorePartNumber = xassemblyPart?.CorePartNumber;
            return assemblyPart;
        }

        public static IList<LaborOperation> GetShallowCopy(IList<LaborOperation> xlaborOperations)
        {
            var laborOperations = new List<LaborOperation>();
            if (xlaborOperations != null)
                foreach (var item in xlaborOperations)
                    laborOperations.Add(GetShallowCopy(item));
            return laborOperations;
        }

        public static LaborOperation GetShallowCopy(LaborOperation xlaborOperation)
        {
            var laborOperation = new LaborOperation();
            laborOperation.Code = xlaborOperation?.Code;
            laborOperation.Description = xlaborOperation?.Description;
            laborOperation.Hours = xlaborOperation?.Hours ?? 0;
            return laborOperation;
        }

        public static Vehicle GetShallowCopy(Vehicle xvehicle)
        {
            var vehicle = new Vehicle();
            vehicle.VIN = xvehicle?.VIN;
            vehicle.Make = xvehicle?.Make;
            vehicle.Model = xvehicle?.Model;
            vehicle.Year = xvehicle?.Year;
            vehicle.UnitInventoryIdentifier = xvehicle?.UnitInventoryIdentifier;
            vehicle.UnitNumber = xvehicle?.UnitNumber;
            vehicle.Condition = xvehicle?.Condition ?? VehicleCondition.Unknown;
            vehicle.Odometer = GetShallowCopy(xvehicle?.Odometer);
            vehicle.Engine = GetShallowCopy(xvehicle?.Engine);
            vehicle.ExternalIdentifiers = GetShallowCopy(xvehicle?.ExternalIdentifiers);
            return vehicle;
        }

        public static Odometer GetShallowCopy(Odometer xodometer)
        {
            var odometer = new Odometer();
            odometer.Reading = xodometer?.Reading ?? 0;
            odometer.UnitType = xodometer?.UnitType ?? OdometerUnitType.UNKNOWN;
            return odometer;
        }

        public static Engine GetShallowCopy(Engine xengine)
        {
            var engine = new Engine();
            engine.Hours = xengine?.Hours;
            return engine;
        }

        public static Appointment GetShallowCopy(Appointment xappointment)
        {
            var appointment = new Appointment();
            appointment.AppointmentMadeDateTime = xappointment?.AppointmentMadeDateTime;
            appointment.ArrivalDateTime = xappointment?.ArrivalDateTime;
            return appointment;
        }
    }
}
