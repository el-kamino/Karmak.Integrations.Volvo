namespace Karmak.Integrations.Volvo.React.Contracts
{
    public static class TelemetryKeys {
        public const string Metric = "Metric";
        public const string EntityType = "EntityType";
        public const string RetransmissionCorrelationGuid = "Retransmission.Correlation.Guid";
        public const string Index = "Index";
        public const string Total = "Total";
        public const string RepairOrderIdentifier = "RepairOrder.Number";
        public const string RepairOrderSnapshotId = "RepairOrder.Snapshot.Id";
        public const string PartsInventoryIdentifier = "Parts.Inventory.Id";
        public const string PartsInventoryType = "Parts.Inventory.Type";
        public const string VehicleSalesInvoiceIdentifier = "Invoice.Number";
        public const string PartsSalesInvoiceIdentifier = "Invoice.Number";
        public const string ServiceAppointmentIdentifier = "Appointment.Id";
        public const string CustomerUpdateVolvoPassIdentifier = "Customer.Identifier.VolvoPassID";
        public const string CustomerUpdateVINIdentifier = "Customer.Identifier.VIN";
        public const string DatabaseRecordId = "Cosmos.Record.Id";
        public const string RetransmitWindowStart = "Retransmit.Window.Start";
        public const string RetransmitWindowEnd = "Retransmit.Window.End";
        public const string EntityCount = "Retransmit.Entity.Count";
        public const string ErrorMessage = "Error.Message";
        public const string ClaimCheckUri = "ClaimCheck.Uri";
        public const string ReactInterface = "React.Interface";
        public const string RepairOrderStatus = "RepairOrderStatus";
        public const string BlobName = "Blob.Name";
        public const string BlobUri = "Blob.Uri";
        public const string LastKnownSnapshotSequenceNumber = "LastKnownSequenceNumber";
        public const string OutOfOrderSequenceNumber = "SequenceNumberReceivedOutOfOrder";
        public const string DealerCode = "DealerCode";
        public const string CustomerTypeCode = "CustomerTypeCode";
        public const string ForceProcessing = "ForceProcessing";
    }
}
