using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class DecimalToQuantityTypeTest
    {
        [Fact]
        public void ConvertsDecimalToQuantityType()
        {
            const decimal value = 20.32m;

            var result = new DecimalToQuantityType().Convert(value);

            Assert.Equal(value, result.Value);
        }
    }
}
