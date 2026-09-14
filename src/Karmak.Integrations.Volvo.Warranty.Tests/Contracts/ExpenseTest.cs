using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class ExpenseTest
    {
        [Fact]
        public void expense_calculates_successfully()
        {
            var unitPrice = 5;
            var quantity = 2;
            var expected = (unitPrice * quantity);

            var result = new Expense
            {
                Identifier = "testIdentifier",
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
        public void expense_calculates_missing_core()
        {
            var unitPrice = 5;
            var quantity = 2;
            var expected = unitPrice * quantity;

            var result = new Expense
            {
                Identifier = "testIdentifier",
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
        public void expense_calculates_missing_unitPrice()
        {
            var quantity = 2;

            var result = new Expense
            {
                Identifier = "testIdentifier",
                Quantity = new Quantity
                {
                    Value = quantity,
                    Type = UnitOfMeasureType.Count
                }
            };

            Assert.Null(result.Total);
        }

        [Fact]
        public void expense_calculates_missing_quantity()
        {
            var unitPrice = 5;

            var result = new Expense
            {
                Identifier = "testIdentifier",
                UnitPrice = new Money
                {
                    Value = unitPrice,
                    Currency = CurrencyCode.USD
                }
            };

            Assert.Null(result.Total);
        }
    }
}
