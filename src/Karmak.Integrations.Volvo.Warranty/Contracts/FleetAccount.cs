namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class FleetAccount
    {
        public string Identifier { get; set; }
        public string PurchaseOrderIdentifier { get; set; }
        public decimal PartsDiscountPercentage { get; set; }
        public decimal LaborDiscountPercentage { get; set; }
        public decimal MiscellaneousDiscountPercentage { get; set; }
        public Money ApprovedAmount { get; set; }
    }
}
