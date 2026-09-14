using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToTextTypeTest
    {
        [Fact]
        public void ConvertsStringToTextType()
        {
            const string value = "test";

            var result = new StringToTextType().Convert(value);

            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void TextTypeConversionHandlesNull()
        {
            var result = new StringToTextType().Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void TextTypeConversionHandlesEmptyString()
        {
            var result = new StringToTextType().Convert("");

            Assert.Null(result);
        }
    }
}
