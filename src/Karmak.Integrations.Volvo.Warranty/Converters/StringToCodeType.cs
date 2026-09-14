using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToCodeType : IConvertible<string, CodeType>
    {
        public CodeType Convert(string source) =>
            Conversions.GetOrNull(source, s => !string.IsNullOrEmpty(s), s => new CodeType { Value = s });
    }
}
