using System;

namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Data
{
    public class MiscCharge {
        public int? MiscellaneousChargeID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MiscellaneousChargeType { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public int Id { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
