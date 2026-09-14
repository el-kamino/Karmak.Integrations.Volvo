using Karmak.Integrations.Volvo.React.Contracts.Common;
using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class RepairOrderSnapshot: IOverridable {
        public Guid SnapshotId { get; set; }
        public int RepairOrderID { get; set; }
        public int SnapshotSequenceNumber { get; set; }
        public DateTime SnapshotSequenceNumberDateTime { get; set; }
        public bool ForceTransmission { get; set; }
        public FusionIdentityInfo FusionIdentityInfo { get; set; }
        public DealerInfo DealerInfo { get; set; }
        public IList<Address> Addresses { get; set; }
        public string DatabaseVersion { get; set; }
        public Customer OwningCustomer { get; set; }
        public Customer BillingCustomer { get; set; }
        public Contact Driver { get; set; }
        public IList<RepairOrderTask> Tasks { get; set; }
        public string RepairOrderNumber { get; set; }
        public string CustomerPONumber { get; set; }
        public Vehicle Vehicle { get; set; }
        public Appointment Appointment { get; set; }
        public string CustomerContactedStatus { get; set; }
        public string ServiceWriterName { get; set; }
        public decimal? MeterReading { get; set; }
        public string MeterType { get; set; }
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
        public string InvoiceIdentifier { get; set; }
        public decimal? TimeZone { get; set; }
        public string KeyTag { get; set; }
        public string PaymentMethod { get; set; }

        public RepairOrderSnapshot() {
            Addresses = new List<Address>();
            Tasks = new List<RepairOrderTask>();
            SnapshotId = Guid.NewGuid();
        }

        public IList<ExternalIdentifier> ExternalIdentifiers { get; set; }
        public IList<ExternalIdentifier> OriginalRepairOrderExternalIdentifiers { get; set; }
    }
}