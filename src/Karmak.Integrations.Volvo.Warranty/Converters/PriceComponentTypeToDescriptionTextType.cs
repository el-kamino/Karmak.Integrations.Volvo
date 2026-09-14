using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class PriceComponentTypeToDescriptionTextType : IConvertible<PricingComponentType, TextType>
    {
        private static readonly IDictionary<PricingComponentType, string> PricePartDescriptionMappings = new Dictionary<PricingComponentType, string>
        {
            [PricingComponentType.Core] = ConstantSettings.CorePartPricingDescription,
            [PricingComponentType.Extended] = ConstantSettings.ExtendedPartPricingDescription,
        };

        public TextType Convert(PricingComponentType source) => new TextType
        {
            Value = PricePartDescriptionMappings[source]
        };
    }
}
