namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class Expense
    {
        public string Identifier { get; set; }
        public string Description { get; set; }
        public Money UnitPrice { get; set; }
        public Quantity Quantity { get; set; }
        public virtual Money Total =>
            UnitPrice == null || Quantity == null
                ? null
                : new Money
                {
                    Currency = UnitPrice.Currency,
                    Value = UnitPrice.Value * Quantity.Value
                };
        public string ThirdPartyInvoiceIdentifier { get; set; }
        public string AppealCode { get; set; }
    }
}
