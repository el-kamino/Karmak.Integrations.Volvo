using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class StringToLanguageEnumeratedTypeTest
    {
        [Theory]
        [InlineData("en-US", LanguageEnumeratedType.enUS)]
        [InlineData("en-us", LanguageEnumeratedType.enUS)]
        [InlineData("enUS", LanguageEnumeratedType.enUS)]
        [InlineData("enus", LanguageEnumeratedType.enUS)]
        public void WhenCastingLanguageTag_It_ReturnsLanguageType(string languageTag, LanguageEnumeratedType languageType)
        {
            var result = new StringToLanguageEnumeratedType().Convert(languageTag);

            Assert.Equal(languageType, result);
        }

        [Fact]
        public void WhenCastingInvalidLanguageTag_It_Throws()
        {
            var ex = Assert.Throws<ConversionException<string, LanguageEnumeratedType>>(() => new StringToLanguageEnumeratedType().Convert("bippity"));

            Assert.Equal("Cannot cast value 'bippity' of type 'System.String' to 'Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5.LanguageEnumeratedType'", ex.Message);
        }
    }
}
