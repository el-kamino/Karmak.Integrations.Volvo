using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class AmountTypeToMoney : IConvertible<AmountType, Money>
    {
        public Money Convert(AmountType source)
        {
            return source == null ? null
                : new Money
                {
                    Value = source.Value,
                    Currency = Conversions.CurrencyIdToCurrencyCode.Convert(source.currencyID)
                };
        }
    }
}
