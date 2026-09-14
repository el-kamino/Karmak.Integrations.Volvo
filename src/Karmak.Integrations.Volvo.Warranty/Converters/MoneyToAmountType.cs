using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public class MoneyToAmountType : IConvertible<IMoney, AmountType>
    {
        private readonly string _currencyCodeAsString;

        public MoneyToAmountType(CurrencyCode currencyCode)
        {
            _currencyCodeAsString = Conversions.EnumToString.Convert(currencyCode);
        }

        public AmountType Convert(IMoney source) =>
            Conversions.GetOrNull(source, s => new AmountType
            {
                currencyID = _currencyCodeAsString,
                Value = decimal.Round(source.Value, 2, System.MidpointRounding.AwayFromZero)
            });

    }
}
