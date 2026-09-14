namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data
{
    public class CurrentPeriodData {
        public decimal QuantityOnHand { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal QuantityAdjustment { get; set; }
        public QuantityAdjustmentDescriptionType AdjustmentDescription { get; set; }
    }
}