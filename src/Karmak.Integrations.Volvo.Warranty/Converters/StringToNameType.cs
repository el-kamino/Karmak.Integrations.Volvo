using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToNameType : IConvertible<string, NameType>
    {
        public NameType Convert(string source) =>
            Conversions.GetOrNull(source, s => new NameType { Value = s });
    }
}
