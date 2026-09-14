using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsSalesOrder
{
    public class FusionPartInventory {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string CorePartNumber { get; set; }
        public decimal? QuantityOnHand { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? LastSold { get; set; }
        public decimal? StockStatus { get; set; }
        public decimal? StockingLevel { get; set; }
        public string PartType { get; set; }
        public IList<FusionAssemblyPart> AssemblyParts { get; set; }
        public IList<FusionAssemblyMiscCharge> AssemblyMiscCharges { get; set; }
    }
}
