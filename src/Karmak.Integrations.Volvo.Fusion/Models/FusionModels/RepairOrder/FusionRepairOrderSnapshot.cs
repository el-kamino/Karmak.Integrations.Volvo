using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder
{
    public class FusionRepairOrderSnapshot {
        public string RepairOrderID { get; set; }
        public decimal? ROTotal { get; set; }
        public decimal? ROTaxAmountTotal { get; set; }
        public string RepairOrderStatus { get; set; }
        public string SubStatus { get; set; }
        public DateTime? PromisedDate { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InServiceDate { get; set; }
        public DateTime? FusionSchedulingDate { get; set; }
        public string Department { get; set; }
        public int? DepartmentID { get; set; }
        public string OriginalRepairOrderNumber { get; set; }
        public string OriginalRepairOrderID { get; set; }
        public string InvoiceNumber { get; set; }
        public string MeterType { get; set; }
        public decimal? MeterReading { get; set; }
        public string CustomerContactedStatus { get; set; }
        public IList<FusionAddress> Addresses { get; set; }
        public string DatabaseVersion { get; set; }
        public FusionCustomer OwningCustomer { get; set; }
        public FusionCustomer BillingCustomer { get; set; }
        public FusionContact Driver { get; set; }
        public IList<FusionRepairOrderTask> Tasks { get; set; }
        public string RepairOrderNumber { get; set; }
        public string CustomerPONumber { get; set; }
        public FusionVehicle Vehicle { get; set; }
        public FusionAppointment Appointment { get; set; }
        public string ServiceWriterName { get; set; }
        public string KeyTag { get; set; }
        public string PaymentMethod { get; set; }
    }
}