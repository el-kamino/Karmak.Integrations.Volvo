using System;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class StringToCountryEnumeratedType : IConvertible<string, CountryEnumeratedType>
    {
        public CountryEnumeratedType Convert(string source) =>
            Enum.TryParse(source?.ToUpper().Replace("USA", "US"), true, out CountryEnumeratedType result)
                ? result
                : throw new ConversionException<string, CountryEnumeratedType>(source);
    }
}
