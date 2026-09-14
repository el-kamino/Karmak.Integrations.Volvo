using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToNameTypeTest
    {
        [Fact]
        public void ConvertsStringToNameType()
        {
            const string value = "test";

            var result = new StringToNameType().Convert(value);

            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void NameTypeConversionHandlesNull()
        {
            var result = new StringToNameType().Convert(null);

            Assert.Null(result);
        }
    }
}
