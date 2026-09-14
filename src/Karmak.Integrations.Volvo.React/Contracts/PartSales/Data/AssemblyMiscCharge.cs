namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Data
{
    public class AssemblyMiscCharge {
        public int? MiscellaneousChargeID { get; set; }
        public string MiscellaneousChargeType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitListPrice { get; set; }
        public decimal? UnitCost { get; set; }
    }
}