using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.PartsSalesOrders.V5_14_4
{
    public static class PartsMapper
    {
        private const int PART_DESCRIPTION_MAX_LENGTH = 50;

        public static PartsInvoiceLineType[] Map(IList<Part> parts, IList<MiscCharge> miscCharges, bool isCanceled, VolvoSettings settings, decimal? tz)
        {
            var mappedRegularParts = MapRegularParts(parts, settings.RegionSettings.CurrencyCode, isCanceled, tz);
            var mappedPartsKits = MapPartsKits(parts, settings.RegionSettings.CurrencyCode, isCanceled, tz);
            var mappedMiscCharges = MapMiscCharges(miscCharges, settings, isCanceled, tz);
            var mapped = mappedRegularParts.Concat(mappedPartsKits).Concat(mappedMiscCharges);

            return mapped.ToArray();
        }

        private static IEnumerable<PartsInvoiceLineType> MapMiscCharges(IList<MiscCharge> miscCharges, VolvoSettings settings, bool isCanceled, decimal? tz)
        {
            var currencyId = settings.RegionSettings.CurrencyCode;
            return miscCharges?.Where(miscCharge => IsNotShippingCharge(miscCharge, settings)).Select(miscCharge => new PartsInvoiceLineType
            {
                PartsProductItem = new PartsProductItemType
                {
                    PartName = new[] {
                        new TextType {
                            Value = miscCharge.Description.MaxLength(PART_DESCRIPTION_MAX_LENGTH)
                        }
                    },
                    ItemIdentificationGroup = new FormattedPartNumberOtcSales(miscCharge.Name).ToItemIdentificationGroup()
                },
                OrderQuantity = MapOrderQuantity(miscCharge.Quantity),
                Price = new[] {
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.UnitPrice,
                        ChargeAmount = new AmountType {
                            Value = miscCharge.UnitPrice.WithDecimalImplied().OrMax(Maximums.FourteenNines).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    },
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.PartCost,
                        ChargeAmount = new AmountType {
                            Value = Math.Abs(miscCharge.UnitCost.WithDecimalImplied().OrMax(Maximums.FourteenNines)).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    }
                },
                PartAddedDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(miscCharge.AddDate, tz),
                PartAddedDateTimeSpecified = miscCharge.AddDate != null,
                ProcessCode = MapProcessCode(miscCharge.Quantity)
            }); ;
        }

        private static bool IsNotShippingCharge(MiscCharge miscCharge, VolvoSettings settings)
        {
            return !(settings.InterfaceOptions.ShippingCharges.Contains(miscCharge.Id));
        }

        private static IEnumerable<PartsInvoiceLineType> MapRegularParts(IList<Part> parts, string currencyId, bool isCanceled, decimal? tz)
        {
            return parts?.Where(part => part.AssemblyParts.IsEmpty()).Select(part => new PartsInvoiceLineType
            {
                PartsProductItem = new PartsProductItemType
                {
                    PartName = new[] {
                        new TextType {
                            Value = MapPartDescription(part, parts)
                        }
                    },
                    ItemIdentificationGroup = new FormattedPartNumberOtcSales(part.Number).ToItemIdentificationGroup()
                },
                OrderQuantity = MapOrderQuantity(part.Quantity),
                Price = new[] {
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.UnitPrice,
                        ChargeAmount = new AmountType {
                            Value = part.UnitPrice.WithDecimalImplied().OrMax(Maximums.FourteenNines).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    },
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.PartCost,
                        ChargeAmount = new AmountType {
                            Value = Math.Abs(part.PartCost.WithDecimalImplied().OrMax(Maximums.FourteenNines)).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    }
                },
                PartAddedDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(part.AddDate, tz),
                PartAddedDateTimeSpecified = part.AddDate != null,
                ProcessCode = MapProcessCode(part.Quantity),
                Note = MapCoreNote(part.PartType)
            });
        }

        private static string MapPartDescription(Part part, IEnumerable<Part> otherParts)
        {
            if (part.PartType == PartType.CORE || otherParts.Select(p => p.CorePartNumber).Contains(part.Number))
            {
                return part.Quantity > 0
                    ? $"{PartConstants.CORE_SALE} {part.Description}".MaxLength(PART_DESCRIPTION_MAX_LENGTH)
                    : $"{PartConstants.CORE_RETURN} {part.Description}".MaxLength(PART_DESCRIPTION_MAX_LENGTH);
            } else
            {
                return part.Description.MaxLength(PART_DESCRIPTION_MAX_LENGTH);
            }
        }

        private static IEnumerable<PartsInvoiceLineType> MapPartsKits(IList<Part> parts, string currencyId, bool isCanceled, decimal? tz)
        {
            var partsKits = parts?.Where(part => part.AssemblyParts.Any()) ?? new List<Part>();
            var mappedPartsKits = new List<PartsInvoiceLineType>();

            foreach (var partsKit in partsKits)
            {
                mappedPartsKits.Add(MapPartKitMainPart(partsKit, currencyId, tz));

                var discountPercentage = ComputeDiscountPercentageForAssembly(partsKit);
                var priceAmountApplied = 0.0m;

                foreach (var assemblyPart in partsKit.AssemblyParts)
                {
                    mappedPartsKits.Add(MapPartsKitPart(assemblyPart, partsKit, currencyId, discountPercentage, isCanceled, tz));
                    priceAmountApplied += ComputeAssemblyExtendedDiscountedPrice(assemblyPart.UnitPrice, assemblyPart.Quantity, discountPercentage);
                }

                foreach (var miscCharge in partsKit.AssemblyMiscCharges)
                {
                    mappedPartsKits.Add(MapPartsKitCharge(miscCharge, partsKit, currencyId, discountPercentage, isCanceled, tz));
                    priceAmountApplied += ComputeAssemblyExtendedDiscountedPrice(miscCharge.UnitListPrice, miscCharge.Quantity, discountPercentage);
                }

                var remainder = ComputeRemainder(partsKit, priceAmountApplied);
                mappedPartsKits.Last().Price[0].ChargeAmount.Value += remainder.WithDecimalImplied(0).IsCanceled(isCanceled);
            }
            return mappedPartsKits;
        }

        private static PartsInvoiceLineType MapPartKitMainPart(Part part, string currencyId, decimal? tz)
        {
            return new PartsInvoiceLineType
            {
                PartsProductItem = new PartsProductItemType
                {
                    PartName = new[] {
                        new TextType {
                            Value = $"{PartConstants.PART_KIT} {part.Description}".MaxLength(PART_DESCRIPTION_MAX_LENGTH)
                        }
                    },
                    ItemIdentificationGroup = new FormattedPartNumberOtcSales(part.Number).ToItemIdentificationGroup()
                },
                OrderQuantity = MapOrderQuantity(part.Quantity),
                Price = new[] {
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.UnitPrice,
                        ChargeAmount = new AmountType {
                            Value = 0m,
                            currencyID = currencyId
                        }
                    },
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.PartCost,
                        ChargeAmount = new AmountType {
                            Value = 0m,
                            currencyID = currencyId
                        }
                    }
                },
                PartAddedDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(part.AddDate, tz),
                PartAddedDateTimeSpecified = part.AddDate != null,
                ProcessCode = MapProcessCode(part.Quantity),
                Note = MapCoreNote(part.PartType)
            };
        }

        private static PartsInvoiceLineType MapPartsKitPart(AssemblyPart assemblyPart, Part partKit, string currencyId, decimal? discountPercentage, bool isCanceled, decimal? tz)
        {
            return new PartsInvoiceLineType
            {
                PartsProductItem = new PartsProductItemType
                {
                    PartName = new[] {
                        new TextType {
                            Value = CreateAssemblyPartIdDescription(assemblyPart, partKit).MaxLength(PART_DESCRIPTION_MAX_LENGTH)
                        }
                    },
                    ItemIdentificationGroup = new FormattedPartNumberOtcSales(assemblyPart.Number).ToItemIdentificationGroup()
                },
                OrderQuantity = MapOrderQuantity(CalculatesAssemblyQuantity(assemblyPart.Quantity, partKit)),
                Price = new[] {
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.UnitPrice,
                        ChargeAmount = new AmountType {
                            Value = ComputeAssemblyDiscountedPrice(assemblyPart.UnitPrice, discountPercentage).OrMax(Maximums.FourteenNines).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    },
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.PartCost,
                        ChargeAmount = new AmountType {
                            Value = Math.Abs(assemblyPart.PartCost.WithDecimalImplied().OrMax(Maximums.FourteenNines)).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    }
                },
                PartAddedDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(partKit.AddDate, tz),
                PartAddedDateTimeSpecified = partKit.AddDate != null,
                ProcessCode = MapProcessCode(assemblyPart.Quantity),
                Note = MapCoreNote(assemblyPart.PartType)
            };
        }

        private static PartsInvoiceLineType MapPartsKitCharge(AssemblyMiscCharge assemblyCharge, Part partKit, string currencyId, decimal? discountPercentage, bool isCanceled, decimal? tz)
        {
            return new PartsInvoiceLineType
            {
                PartsProductItem = new PartsProductItemType
                {
                    PartName = new[] {
                        new TextType {
                            Value = $"{PartConstants.PART_KIT} {assemblyCharge.Description}"
                        }
                    },
                    ItemIdentificationGroup = new FormattedPartNumberOtcSales(assemblyCharge.Name).ToItemIdentificationGroup()
                },
                OrderQuantity = MapOrderQuantity(CalculatesAssemblyQuantity(assemblyCharge.Quantity, partKit)),
                Price = new[] {
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.UnitPrice,
                        ChargeAmount = new AmountType {
                            Value = ComputeAssemblyDiscountedPrice(assemblyCharge.UnitListPrice, discountPercentage).OrMax(Maximums.FourteenNines).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    },
                    new PriceABIETypeStar {
                        PriceCodeSpecified = true,
                        PriceCode = PriceEnumeratedType.PartCost,
                        ChargeAmount = new AmountType {
                            Value = Math.Abs(assemblyCharge.UnitCost.GetValueOrDefault().WithDecimalImplied().OrMax(Maximums.FourteenNines)).IsCanceled(isCanceled),
                            currencyID = currencyId
                        }
                    }
                },
                PartAddedDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(partKit.AddDate, tz),
                PartAddedDateTimeSpecified = partKit.AddDate != null,
                ProcessCode = MapProcessCode(assemblyCharge.Quantity),
            };
        }

        private static string CreateAssemblyPartIdDescription(AssemblyPart assemblyPart, Part partKit)
        {
            if (assemblyPart.PartType == PartType.CORE)
            {
                return assemblyPart.Quantity * partKit.Quantity > 0
                    ? $"{PartConstants.PART_KIT} {PartConstants.CORE_SALE} {assemblyPart.Description}"
                    : $"{PartConstants.PART_KIT} {PartConstants.CORE_RETURN} {assemblyPart.Description}";
            }
            return $"{PartConstants.PART_KIT} {assemblyPart.Description}";
        }

        private static decimal CalculatesAssemblyQuantity(decimal? quantity, Part part) =>
            quantity.GetValueOrDefault() * part.Quantity;

        private static decimal ComputeAssemblyExtendedDiscountedPrice(decimal? price, decimal? quantity, decimal? discountPercentage) =>
            (price.GetValueOrDefault() * quantity.GetValueOrDefault() * discountPercentage.GetValueOrDefault()).WithDecimalImplied();

        private static decimal ComputeAssemblyDiscountedPrice(decimal? price, decimal? discountPercentage) =>
            (price.GetValueOrDefault() * discountPercentage.GetValueOrDefault()).WithDecimalImplied();

        private static decimal? ComputeDiscountPercentageForAssembly(Part partKit)
        {
            var partPrice = partKit.AssemblyParts.Sum(p => p.UnitPrice * p.Quantity);
            var chargePrice = partKit?.AssemblyMiscCharges?.Sum(charge => charge.UnitListPrice * charge.Quantity);
            return partKit.UnitPrice / (partPrice + chargePrice);
        }

        private static decimal ComputeRemainder(Part repairOrderPartKit, decimal priceAmountApplied)
        {
            return repairOrderPartKit.UnitPrice.WithDecimalImplied() - priceAmountApplied;
        }

        private static QuantityTypeStarQualified MapOrderQuantity(decimal quantity)
        {
            return new QuantityTypeStarQualified
            {
                Value = quantity.WithDecimalImplied().OrMax(Maximums.TwelveNines)
            };
        }

        private static CodeType MapProcessCode(decimal? qty)
        {
            if (qty < 0)
            {
                return new CodeType
                {
                    Value = "RETURN"
                };
            }
            return null;
        }

        private static NoteType[] MapCoreNote(string partType)
        {
            if (partType == "Core")
            {
                return new[]
                {
                    new NoteType
                    {
                        Value = "CORE"
                    }
                };
            }
            return null;

        }
    }
}