using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class DecimalToAmountTypeTest
    {
        [Fact]
        public void ConvertsDecimalToAmountType()
        {
            const decimal value = 10.32m;

            var result = new DecimalToAmountType(CurrencyCode.USD).Convert(value);

            Assert.Equal("USD", result.currencyID);
            Assert.Equal(value, result.Value);
        }
    }
}
