namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data
{
    public class AssemblyPart {
        public decimal? UnitCost { get; set; }
        public decimal? UnitListPrice { get; set; }
        public decimal? Quantity { get; set; }
        public string Description { get; set; }
        public string PartNumber { get; set; }
        public string PartType { get; set; }
        public string CorePartNumber { get; set; }
    }
}