using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Contracts
{
    public class MiscExpenseTest
    {
        [Theory]
        [MemberData(nameof(QuantityData))]
        public void it_calculates_total_with_null_days(Quantity quantity, Quantity secondQuantity, Money unitPrice, decimal expectedTotal)
        {
            var expense = new MiscellaneousExpense
            {
                Quantity = quantity,
                SecondQuantity = secondQuantity,
                UnitPrice = unitPrice,
            };

            Assert.Equal(expectedTotal, expense.Total.Value);
        }

        public static IEnumerable<object[]> QuantityData => new List<object[]> {
            new object[]{new Quantity { Value = 5, Type = UnitOfMeasureType.Days }, new Quantity { Value = 0, Type = UnitOfMeasureType.Hours }, new Money { Value = 2m, Currency = CurrencyCode.USD }, 10m },
            new object[]{new Quantity { Value = 0, Type = UnitOfMeasureType.Days }, new Quantity { Value = 5, Type = UnitOfMeasureType.Hours }, new Money { Value = 2m, Currency = CurrencyCode.USD }, 10m },
            new object[]{new Quantity { Value = 5, Type = UnitOfMeasureType.Days }, new Quantity { Value = 10000, Type = UnitOfMeasureType.Hours }, new Money { Value = 2m, Currency = CurrencyCode.USD }, 10m },
            new object[]{new Quantity { Value = 0, Type = UnitOfMeasureType.Days }, new Quantity { Value = 0, Type = UnitOfMeasureType.Hours }, new Money { Value = 2m, Currency = CurrencyCode.USD }, 0m },
            new object[]{new Quantity { Value = 10, Type = UnitOfMeasureType.Days }, new Quantity { Value = 10, Type = UnitOfMeasureType.Hours }, new Money { Value = 0m, Currency = CurrencyCode.USD }, 0m },
            new object[]{null, new Quantity { Value = 5, Type = UnitOfMeasureType.Hours }, new Money { Value = 2m, Currency = CurrencyCode.USD }, 10m },
            new object[]{new Quantity { Value = 5, Type = UnitOfMeasureType.Days }, null, new Money { Value = 2m, Currency = CurrencyCode.USD }, 10m },
            new object[]{new Quantity { Value = 5, Type = UnitOfMeasureType.Days }, null, null, 0m },
            new object[]{new Quantity { Value = 0, Type = UnitOfMeasureType.Days }, null, new Money { Value = 2m, Currency = CurrencyCode.USD }, 0m },
            new object[]{null, null, null, 0m },
        };
    }
}
