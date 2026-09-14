using System;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class MiscCharge {
        public int? MiscellaneousChargeID { get; set; }
        public string MiscellaneousChargeType { get; set; }
        public bool? IncludeMiscellaneousChargeType { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? UnitPrice { get; set; }
        public string InvoiceIdentifier { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}