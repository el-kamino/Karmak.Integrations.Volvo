using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class PartExpenseTest
    {
        [Fact]
        public void IncludesCorePrice_In_Total()
        {
            var unitPrice = 5;
            var quantity = 2;
            var corePrice = 3;
            var expected = (unitPrice * quantity) + corePrice;

            var result = new PartExpense
            {
                Identifier = "testIdentifier",
                CorePrice = new Money
                {
                    Value = corePrice,
                    Currency = CurrencyCode.USD
                },
                UnitPrice = new Money
                {
                    Value = unitPrice,
                    Currency = CurrencyCode.USD
                },
                Quantity = new Quantity
                {
                    Value = quantity,
                    Type = UnitOfMeasureType.Count
                }
            };

            Assert.Equal(expected, result.Total.Value);
        }

        [Fact]
        public void ExcludesCorePrice_In_ExtendedPrice()
        {
            var unitPrice = 5;
            var quantity = 2;
            var corePrice = 3;
            var expected = (unitPrice * quantity);

            var result = new PartExpense
            {
                Identifier = "testIdentifier",
                CorePrice = new Money
                {
                    Value = corePrice,
                    Currency = CurrencyCode.USD
                },
                UnitPrice = new Money
                {
                    Value = unitPrice,
                    Currency = CurrencyCode.USD
                },
                Quantity = new Quantity
                {
                    Value = quantity,
                    Type = UnitOfMeasureType.Count
                }
            };

            Assert.Equal(expected, result.ExtendedPrice.Value);
        }
    }
}
