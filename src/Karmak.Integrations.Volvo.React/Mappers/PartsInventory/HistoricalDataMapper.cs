using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public static class HistoricalDataMapper
    {
        public static QuantitySoldHistoryType[] Map(InventoryPart part)
        {
            return part.QuantitySoldHistory.Select(data => new QuantitySoldHistoryType
            {
                PeriodID = new IdentifierType
                {
                    Value = data.Period
                },
                QuantitySold = new QuantityTypeStarQualified
                {
                    Value = data.QuantitySold.Truncate().OrMax(Maximums.SevenNines)
                }
            }).ToArray();
        }
    }
}


