using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class Part {
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
        public IList<AssemblyPart> AssemblyParts { get; set; }
        public IList<AssemblyMiscCharge> AssemblyMiscCharges { get; set; } = new List<AssemblyMiscCharge>();
        public string InvoiceIdentifier { get; set; }
        public DateTime? InvoiceDate { get; set; }

        public Part() {
            AssemblyParts = new List<AssemblyPart>();
        }
    }
}