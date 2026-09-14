using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class ShippingChargesMapper
    {
        public static ChargesType Map(PartsSalesOrder partsSalesOrder, VolvoSettings settings, bool isCanceled)
        {
            var currencyID = settings.RegionSettings.CurrencyCode;
            var shippingCharges = CalculateTotalShipping(partsSalesOrder.MiscCharges, settings.InterfaceOptions.ShippingCharges);
            return shippingCharges == 0m || isCanceled
            ? null
            : new ChargesType
            {
                DeliveryChargeAmount = new AmountType
                {
                    Value = shippingCharges.WithDecimalImplied().OrMax(Maximums.FourteenNines),
                    currencyID = currencyID
                }
            };
        }

        private static decimal CalculateTotalShipping(IEnumerable<MiscCharge> miscCharges, int[] shippingCharges)
        {
            return miscCharges?
                .Where(charge => shippingCharges.Contains(charge.Id))
                .Sum(charge => charge.UnitPrice * charge.Quantity) ?? 0m;
        }
    }
}