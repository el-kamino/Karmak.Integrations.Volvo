using System;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder
{
    public class FusionMiscCharge {
        public int? MiscellaneousChargeID { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitCost { get; set; }
        public string MiscPONumber { get; set; }
        public DateTime? MiscPODate { get; set; }
        public string MiscellaneousChargeType { get; set; }
        public bool? IncludeMiscellaneousChargeType { get; set; }
        public DateTime? AddedDateTime { get; set; }
        public string DataState { get; set; }
    }
}