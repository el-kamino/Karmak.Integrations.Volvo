using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToTextType : IConvertible<string, TextType>
    {
        public TextType Convert(string source) =>
            Conversions.GetOrNull(source, s => !string.IsNullOrEmpty(s), s => new TextType { Value = s });
    }
}
