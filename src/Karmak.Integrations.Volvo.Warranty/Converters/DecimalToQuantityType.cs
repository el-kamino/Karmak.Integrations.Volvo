using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class DecimalToQuantityType : IConvertible<decimal, QuantityType>
    {
        public QuantityType Convert(decimal source) =>
            Conversions.GetOrNull(source, s => new QuantityType { Value = s });
    }
}
