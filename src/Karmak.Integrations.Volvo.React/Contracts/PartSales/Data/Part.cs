using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Data
{
    public class Part {
        public string Number { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PartCost { get; set; }
        public string PartType { get; set; }
        public string CorePartNumber { get; set; }
        public DateTime? AddDate { get; set; }
        public IList<AssemblyPart> AssemblyParts { get; set; }
        public IList<AssemblyMiscCharge> AssemblyMiscCharges { get; set; } = new List<AssemblyMiscCharge>();
    }
}