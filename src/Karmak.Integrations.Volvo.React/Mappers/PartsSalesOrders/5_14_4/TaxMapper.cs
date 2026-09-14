using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Constants.Shared;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class TaxMapper
    {
        public static TaxType[] Map(decimal taxTotal, string currencyCode, bool isCanceled)
        {
            return taxTotal == 0m || isCanceled
            ? null
            : new[] {
                new TaxType {
                    TaxTypeCode = TaxTypeEnumeratedType.Total,
                    TaxAmount = new AmountType {
                        Value = taxTotal.WithDecimalImplied().OrMax(Maximums.FourteenNines),
                        currencyID = currencyCode
                    }
                }
            };
        }
    }
}