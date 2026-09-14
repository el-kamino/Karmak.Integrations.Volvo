using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class MoneyToAmountTypeTest
    {
        [Fact]
        public void ConvertsMoneyToAmountType()
        {
            var value = new Money
            {
                Currency = CurrencyCode.USD,
                Value = 53.21m
            };

            var result = new MoneyToAmountType(CurrencyCode.USD).Convert(value);

            Assert.Equal("USD", result.currencyID);
            Assert.Equal(value.Value, result.Value);
        }

        [Fact]
        public void AmountTypeConversionIgnoresMoneyCurrency()
        {
            var value = new Money
            {
                Currency = CurrencyCode.CAD,
                Value = 123.45m
            };

            var result = new MoneyToAmountType(CurrencyCode.USD).Convert(value);

            Assert.Equal("USD", result.currencyID);
            Assert.Equal(value.Value, result.Value);
        }

        [Fact]
        public void AmountTypeConversionHandlesNull()
        {
            var result = new MoneyToAmountType(CurrencyCode.USD).Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void AmountTypeConversionRoundsWithBankersRounding()
        {
            var testMoney = new Money
            {
                Currency = CurrencyCode.USD,
                Value = 123.515m
            };

            var result = new MoneyToAmountType(CurrencyCode.USD).Convert(testMoney);

            Assert.Equal(123.52m, result.Value);
        }
    }
}
