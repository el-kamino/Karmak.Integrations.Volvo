using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Converters
{
    public class PriceComponentTypeToDescriptionTextTypeTest
    {
        [Theory]
        [InlineData(PricingComponentType.Core, ConstantSettings.CorePartPricingDescription)]
        [InlineData(PricingComponentType.Extended, ConstantSettings.ExtendedPartPricingDescription)]
        public void WhenGettingPricingComponentDescription_It_ReturnsDescription(PricingComponentType pricingComponent, string description)
        {
            var result = new PriceComponentTypeToDescriptionTextType().Convert(pricingComponent);

            Assert.Equal(description, result.Value);
        }
    }
}
