using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToCodeTypesTest
    {
        [Fact]
        public void ConvertsStringsToCodeTypes()
        {
            var values = new[] { "testOne", "testTwo", "testThree" };

            var result = new StringToCodeTypes().Convert(values);

            Assert.Equal(values, result.Select(value => value.Value).ToArray());
        }

        [Fact]
        public void CodeTypesConversionHandlesNull()
        {
            var result = new StringToCodeTypes().Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void CodeTypesConversionHandlesEmptyStrings()
        {
            var values = new[] { "testOne", "", "testThree" };

            var result = new StringToCodeTypes().Convert(values);

            Assert.Equal(new[] { "testOne", "testThree" }, result.Select(value => value.Value).ToArray());
        }
    }
}
