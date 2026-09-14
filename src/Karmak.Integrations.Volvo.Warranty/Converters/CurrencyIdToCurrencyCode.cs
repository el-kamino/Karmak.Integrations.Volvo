using System;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class CurrencyIdToCurrencyCode : IConvertible<string, CurrencyCode>
    {
        public CurrencyCode Convert(string source) => CastToCurrencyCode(source);

        private CurrencyCode CastToCurrencyCode(string x) =>
            Enum.TryParse(x, out CurrencyCode result)
                                ? result
                                : CurrencyCode.USD;
    }
}
