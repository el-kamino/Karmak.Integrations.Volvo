using System;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToLanguageEnumeratedType : IConvertible<string, LanguageEnumeratedType>
    {
        public LanguageEnumeratedType Convert(string source) =>
            Enum.TryParse(source.Replace("-", string.Empty), true, out LanguageEnumeratedType result)
                ? result
                : throw new ConversionException<string, LanguageEnumeratedType>(source);
    }
}
