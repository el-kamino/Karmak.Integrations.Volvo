using System;
using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Mappers.PartsInventory
{
    public static class InventoryLineMapper
    {
        public static PartsInventoryLineType[] Map(PartsInventoryReport partsInventoryReport, VolvoSettings settings)
        {
            return partsInventoryReport.Parts.Select(part =>
            {
                return new PartsInventoryLineType
                {
                    QuantityOnHand = new QuantityTypeStarQualified
                    {
                        Value = part.DataForCurrentPeriod.QuantityOnHand.Truncate().OrMax(Maximums.SevenNines)
                    },
                    QuantitySold = new QuantityTypeStarQualified
                    {
                        Value = part.DataForCurrentPeriod.QuantitySold.Truncate().OrMax(Maximums.SevenNines)
                    },
                    LastSoldDate = part.LastSoldDate.GetValueOrDefault().Date,
                    LastSoldDateSpecified = part.LastSoldDate.HasValue,
                    StockingStatusCode = StockingStatusMapper.Map(part),
                    QuantityBestStockingLevel = new QuantityTypeStarQualified
                    {
                        Value = Math.Abs(part.QuantityBestStockingLevel).Truncate().OrMax(Maximums.SevenNines)
                    },
                    PartsProductItem = new PartsProductItemType
                    {
                        ItemIdentificationGroup = new FormattedPartNumberWithPackedServicePart(part.PartNumber)
                            .ToItemIdentificationGroup(),
                    },
                    PartsForecasting = new PartsForecastingType
                    {
                        QuantitySoldHistory = HistoricalDataMapper.Map(part)
                    },
                    LastReceiptQuantity = new QuantityTypeStarQualified
                    {
                        Value = part.DataForCurrentPeriod.QuantityReceived.Truncate().OrMax(Maximums.SevenNines)
                    },
                    PartCostAmount = new AmountType
                    {
                        currencyID = settings.RegionSettings.CurrencyCode,
                        Value = part.ExtendedDealerCost.WithDecimalImplied().OrMax(Maximums.FourteenNines)
                    },
                    QuantityAdjustmentDaily = IncludesQuantityAdjustment(part.DataForCurrentPeriod)
                        ? new QuantityTypeOagisUnqualified {
                            Value = part.DataForCurrentPeriod.QuantityAdjustment.Truncate().OrMax(Maximums.SevenNines)
                        }
                        : null,
                    QuantityAdjustmentDailyDescription = IncludesQuantityAdjustment(part.DataForCurrentPeriod)
                        ? new TextType
                        {
                            Value = part.DataForCurrentPeriod.AdjustmentDescription.ToString().ToUpper()
                        }
                        : null,
                    BinLocation = string.IsNullOrWhiteSpace(part.BinLocation)
                        ? null 
                        : new TextType[]
                        {
                            new TextType
                            {
                                Value = part.BinLocation
                            }
                        }
                };
            }).ToArray();
        }

        private static bool IncludesQuantityAdjustment(CurrentPeriodData data) {
            return data != null && data.QuantityAdjustment != 0;
        }
    }
}