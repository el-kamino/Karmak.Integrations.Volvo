namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class PartExpense : Expense
    {
        public string Prefix { get; set; }
        public string Number { get; set; }
        public string Suffix { get; set; }
        public string ConditionCode { get; set; }
        public bool IsCausalPart { get; set; }
        public Money UnitCost { get; set; }
        public Money CorePrice { get; set; }
        public Money ExtendedPrice =>
            UnitPrice == null || Quantity == null
                ? null
                : new Money
                {
                    Currency = UnitPrice.Currency,
                    Value = (UnitPrice.Value * Quantity.Value)
                };
        public override Money Total =>
            ExtendedPrice == null
                ? null
                : new Money
                {
                    Currency = ExtendedPrice.Currency,
                    Value = ExtendedPrice.Value + (CorePrice?.Value ?? 0)
                };
    }
}
