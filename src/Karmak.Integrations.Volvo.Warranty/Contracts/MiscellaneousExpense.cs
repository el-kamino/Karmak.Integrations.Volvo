namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class MiscellaneousExpense : Expense
    {
        public Quantity SecondQuantity { get; set; }
        public override Money Total
        {
            get => CalculateTotal();
        }
        private Money CalculateTotal()
        {
            decimal value = 0;
            if (this?.Quantity?.Value != 0 && this?.Quantity?.Value != null)
            {
                value = Quantity.Value;
            }
            else if (this?.SecondQuantity?.Value != 0 && this?.SecondQuantity?.Value != null)
            {
                value = SecondQuantity.Value;
            }
            value = value * (this?.UnitPrice?.Value ?? 0);
            return new Money()
            {
                Value = value,
                Currency = this?.UnitPrice?.Currency ?? CurrencyCode.USD
            };
        }
    }
}
