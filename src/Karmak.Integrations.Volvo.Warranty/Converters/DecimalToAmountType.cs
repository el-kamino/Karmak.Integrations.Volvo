using System;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class DecimalToAmountType : IConvertible<decimal, AmountType>
    {
        private readonly string _currencyCodeAsString;

        public DecimalToAmountType(CurrencyCode currencyCode)
        {
            _currencyCodeAsString = Conversions.EnumToString.Convert(currencyCode);
        }

        public AmountType Convert(decimal source) =>
            Conversions.GetOrNull(source, s => new AmountType
            {
                currencyID = _currencyCodeAsString,
                Value = Math.Round(s, 2)
            });
    }
}
