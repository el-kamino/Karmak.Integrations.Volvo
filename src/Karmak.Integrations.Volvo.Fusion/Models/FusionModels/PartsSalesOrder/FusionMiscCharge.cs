using System;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder
{
    public class FusionMiscCharge {
        public string Name { get; set; }
        public string Description { get; set; }
        public string MiscellaneousChargeType { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExtendedPrice { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public int MiscellaneousChargeID { get; set; }
        public DateTime? AddDateTime { get; set; }
    }
}
