using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToIdentifierType : IConvertible<string, IdentifierType>
    {
        public IdentifierType Convert(string source) =>
            Conversions.GetOrNull(source, s => !string.IsNullOrEmpty(s), s => new IdentifierType { Value = s });
    }
}
