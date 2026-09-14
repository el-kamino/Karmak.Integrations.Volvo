using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.RepairOrder
{
    public class FusionPart {
        public string AddUsername { get; set; }
        public DateTime? AddDate { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? ExtendedCost { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public decimal? CoreUnitCost { get; set; }
        public decimal? CoreUnitPrice { get; set; }
        public decimal? CoreExtendedPrice { get; set; }
        public decimal? CoreExtendedCost { get; set; }
        public decimal? CoreQuantity { get; set; }
        public string PartType { get; set; }
        public string CorePartNumber { get; set; }
        public IList<FusionAssemblyPart> AssemblyParts { get; set; }
        public IList<FusionAssemblyMiscCharge> AssemblyMiscCharges { get; set; }
        public string MiscPONumber { get; set; }
        public DateTime? MiscPODate { get; set; }
    }
}