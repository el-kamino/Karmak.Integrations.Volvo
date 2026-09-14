using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class CountryCodeToCountryEnumeratedTypeTest
    {
        [Theory]
        [InlineData(CountryCode.US, CountryEnumeratedType.US)]
        [InlineData(CountryCode.CA, CountryEnumeratedType.CA)]
        public void WhenCastingCountryCode_It_ReturnsCountryType(CountryCode countryCode, CountryEnumeratedType countryType)
        {
            var result = new CountryCodeToCountryEnumeratedType().Convert(countryCode);

            Assert.Equal(countryType, result);
        }
    }
}
