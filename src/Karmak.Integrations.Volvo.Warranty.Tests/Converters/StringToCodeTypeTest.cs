using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToCodeTypeTest
    {
        [Fact]
        public void ConvertsStringToCodeType()
        {
            var value = "test";

            var result = new StringToCodeType().Convert(value);

            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void CodeTypeConversionHandlesNull()
        {
            var result = new StringToCodeType().Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void CodeTypeConversionHandlesEmptyString()
        {
            var result = new StringToCodeType().Convert("");

            Assert.Null(result);
        }
    }
}
