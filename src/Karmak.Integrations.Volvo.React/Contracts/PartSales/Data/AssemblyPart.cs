namespace Karmak.Integrations.Volvo.React.Contracts.PartSales.Data
{
    public class AssemblyPart {
        public string Number { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string PartType { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PartCost { get; set; }
    }
}
