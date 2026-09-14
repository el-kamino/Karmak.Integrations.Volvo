using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToIdentifierTypeTest
    {
        [Fact]
        public void ConvertsStringToIdentifierType()
        {
            const string value = "test";

            var result = new StringToIdentifierType().Convert(value);

            Assert.Equal(value, result.Value);
        }

        [Fact]
        public void IdentifierTypeConversionHandlesNull()
        {
            var result = new StringToIdentifierType().Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void IdentifierTypeConversionHandlesEmptyString()
        {
            var result = new StringToIdentifierType().Convert("");

            Assert.Null(result);
        }
    }
}
