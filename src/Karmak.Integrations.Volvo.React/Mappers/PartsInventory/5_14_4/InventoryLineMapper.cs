using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Mappers.PartsInventory;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.PartsInventory.V5_14_4
{
    public static class InventoryLineMapper
    {
        private const string DEFAULT_BIN_LOCATION = "NOBIN";
        private const string VOLVO_PART_SOURCE_CODE = "F";
        private const string DEFAULT_PART_SOURCE_CODE = "O";
        private const string RIM_MANAGED_REPLENISHMENT_CODE = "Y";
        private const string DEFAULT_REPLENISHMENT_CODE = "N";

        public static PartsInventoryLineType[] Map(PartsInventoryReport partsInventoryReport, string currencyCode)
        {
            var partList = partsInventoryReport.Parts
                .Select(part => BuildPart(part, currencyCode))
                .ToList();
            if (partsInventoryReport.Type == ReportType.Full)
            {
                AdjustForFullReport(partList, currencyCode, partsInventoryReport.Metadata.CreatedAtDateTime);
            }
            return partList.ToArray();
        }

        private static PartsInventoryLineType BuildPart(InventoryPart part, string currencyCode)
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
                    currencyID = currencyCode,
                    Value = part.ExtendedDealerCost.WithDecimalImplied().OrMax(Maximums.FourteenNines)
                },
                QuantityAdjustmentDaily = IncludesQuantityAdjustment(part.DataForCurrentPeriod)
                ? new QuantityTypeOagisUnqualified
                {
                    Value = part.DataForCurrentPeriod.QuantityAdjustment.Truncate().OrMax(Maximums.SevenNines)
                }
                : null,
                QuantityAdjustmentDailyDescription = IncludesQuantityAdjustment(part.DataForCurrentPeriod)
                ? new TextType
                {
                    Value = part.DataForCurrentPeriod.AdjustmentDescription.ToString().ToUpper()
                }
                : null,
                BinLocation = new TextType[]
                {
                    new TextType
                    {
                        Value = string.IsNullOrWhiteSpace(part.BinLocation)
                            ? DEFAULT_BIN_LOCATION
                            : part.BinLocation
                    }
                },
                PartSourceCode = new CodeType[]
                {
                    new CodeType
                    {
                        Value = part.IsVolvoPart
                            ? VOLVO_PART_SOURCE_CODE
                            : DEFAULT_PART_SOURCE_CODE
                    }
                },
                ReplenishmentCode = new CodeType
                {
                    Value = part.IsRimManaged
                        ? RIM_MANAGED_REPLENISHMENT_CODE
                        : DEFAULT_REPLENISHMENT_CODE
                },
                FreeFormTextGroup = new FreeFormTextGroupType
                {
                    Note = new TextType[]
                    {
                        new TextType
                        {
                            Value = part.TriggerReasonCode
                        }
                    }
                }
            };
        }
        private static bool IncludesQuantityAdjustment(CurrentPeriodData data)
        {
            return data != null && data.QuantityAdjustment != 0;
        }

        private static void AdjustForFullReport(List<PartsInventoryLineType> partList, string currencyCode, DateTime? createdAtDateTime)
        {
            int partCount = partList.Count;
            partList.Insert(0, BuildPartForFullReport(createdAtDateTime, partCount, currencyCode, isFirstPart: true));
            partList.Add(BuildPartForFullReport(createdAtDateTime, partCount, currencyCode, isFirstPart: false));
        }
        private static PartsInventoryLineType BuildPartForFullReport(DateTime? createdAtDateTime, int partCount, string currencyCode, bool isFirstPart)
        {
            return new PartsInventoryLineType
            {
                QuantityOnHand = new QuantityTypeStarQualified
                {
                    Value = partCount + 2
                },
                QuantitySold = new QuantityTypeStarQualified
                {
                    Value = 0
                },
                LastSoldDate = createdAtDateTime.GetValueOrDefault().Date,
                LastSoldDateSpecified = createdAtDateTime.HasValue,
                StockingStatusCode = StockingStatusMapper.Map(StockingStatus.NonStocked),
                QuantityBestStockingLevel = null,
                PartsProductItem = new PartsProductItemType
                {
                    ItemIdentificationGroup = BuildItemIdentificationGroupForFullReportPart(isFirstPart)
                },
                PartsForecasting = new PartsForecastingType
                {
                    QuantitySoldHistory = BuildSaleHistoryForFullReportPart()
                },
                LastReceiptQuantity = new QuantityTypeStarQualified
                {
                    Value = 0
                },
                PartCostAmount = new AmountType
                {
                    currencyID = currencyCode,
                    Value = 0
                },
                QuantityAdjustmentDaily = null,
                QuantityAdjustmentDailyDescription = null,
                BinLocation = new TextType[]
                {
                    new TextType
                    {
                        Value = DEFAULT_BIN_LOCATION
                    }
                },
                PartSourceCode = new CodeType[]
                {
                    new CodeType
                    {
                        Value = DEFAULT_PART_SOURCE_CODE
                    }
                },
                ReplenishmentCode = new CodeType
                {
                    Value = DEFAULT_REPLENISHMENT_CODE
                },
                FreeFormTextGroup = new FreeFormTextGroupType
                {
                    Note = new TextType[]
                    {
                        new TextType {
                            Value = "FLL"
                        }
                    }
                }
            };
        }
        private static ItemIdentificationType[] BuildItemIdentificationGroupForFullReportPart(bool isFirstPart)
        {
            string prefix = isFirstPart ? "START" : "END";
            var list = new List<ItemIdentificationType>()
            {
                new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeName = "Part Prefix",
                        Value = prefix
                    }
                },
                new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeName = "Part Base",
                        Value = "FULL"
                    }
                },
                new ItemIdentificationType
                {
                    ItemID = new IdentifierType
                    {
                        schemeName = "Packed Service Part",
                        Value = $"{prefix}FULL"
                    }
                }
            };
            return list.ToArray();
        }
        private static QuantitySoldHistoryType[] BuildSaleHistoryForFullReportPart()
        {
            return Enumerable.Range(0, 13)
                .Select(i => new QuantitySoldHistoryType
                {
                    PeriodID = new IdentifierType
                    {
                        Value = i.ToString()
                    },
                    QuantitySold = new QuantityTypeStarQualified
                    {
                        Value = 0
                    }
                }).ToArray();
        }
    }
}