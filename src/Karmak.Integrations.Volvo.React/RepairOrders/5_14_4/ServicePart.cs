using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Common.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4
{
    public class ServicePart
    {
        private const int SPECIFIED_PERSON_MAX_LENGTH = 10;
        private const string LOCAL = "local";

        private readonly VolvoSettings _volvoSettings;
        private readonly decimal? _timeZone;
        public ServicePart(VolvoSettings volvoSettings, decimal? timeZone)
        {
            _volvoSettings = volvoSettings;
            _timeZone = timeZone;
        }

        public ServicePartsType[] Fetch(RepairOrderTask job) {
            var partsKits = job.Parts.Where(part => part.IsAssemblyPart());
            var exchangeParts = job.Parts.Where(part => part.IsExchangePart());
            var coreParts = job.Parts.Where(part => part.IsCorePart(exchangeParts));

            var alreadySeenPartNumbers = partsKits.Concat(exchangeParts).Concat(coreParts).Select(p => p.PartNumber).ToList();
            var regularParts = job.Parts.Where(part => !alreadySeenPartNumbers.Contains(part.PartNumber)).ToList();
            var serviceParts = regularParts
                .Select(ForRegular)
                .ToList();

            foreach (var exchange in exchangeParts)
            {
                serviceParts.AddRange(ForExchange(exchange));
                var returnedCore = coreParts.FirstOrDefault(core => core.PartNumber.Equals(exchange.CorePartNumber));
                if (returnedCore == null)
                {
                    continue;
                }
                returnedCore.PartNumber = exchange.PartNumber;
                returnedCore.Description = GetPartDescription(exchange);
                serviceParts.Add(ForCoreReturn(returnedCore));
                returnedCore.PartNumber = exchange.CorePartNumber;
            }

            serviceParts.AddRange(coreParts
                                      .Where(core => !exchangeParts.Select(part => part.CorePartNumber).Contains(core.PartNumber))
                                      .Select(x => ForCoreReturn(x)));

            serviceParts.AddRange(partsKits.SelectMany(ForPartsKit));

            return serviceParts.ToArray();
        }

        private ServicePartsType ForRegular(Part repairOrderPart)
        {
            if (repairOrderPart == null)
            {
                return null;
            }

            return new ServicePartsType
            {
                CodesAndCommentsExpanded = new CodesAndCommentsExpandedType(),
                ItemIdDescription = new[] {
                    new TextType {
                        Value = GetPartDescription(repairOrderPart)
                    }
                },
                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = repairOrderPart.Quantity.GetValueOrDefault().WithDecimalImplied()
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = repairOrderPart.Quantity < 0
                                    ? new AmountType {
                                        Value = -Math.Abs(repairOrderPart.ExtendedPrice.GetValueOrDefault().WithDecimalImplied()),
                                        currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                    }
                                    : new AmountType {
                                        Value = Math.Abs(repairOrderPart.ExtendedPrice.GetValueOrDefault().WithDecimalImplied()),
                                        currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                    },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = repairOrderPart.UnitCost.GetValueOrDefault().WithDecimalImplied(),
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(repairOrderPart.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPart.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPart.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPart.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                }
            };
        }

        private ServicePartsType ForPartSale(Part repairOrderPart)
        {
            if (repairOrderPart == null)
            {
                return null;
            }
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = GetPartDescription(repairOrderPart)
                    }
                },
                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = Math.Abs(repairOrderPart.Quantity.GetValueOrDefault().WithDecimalImplied())
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = Math.Abs(repairOrderPart.ExtendedPrice.GetValueOrDefault().WithDecimalImplied())
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = repairOrderPart.UnitCost.GetValueOrDefault().WithDecimalImplied()
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(repairOrderPart.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPart.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPart.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPart.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                }
            };
        }

        private ServicePartsType ForCoreSale(Part repairOrderPart)
        {
            if (repairOrderPart == null)
            {
                return null;
            }
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = PartConstants.CORE_SALE + " " + GetPartDescription(repairOrderPart)
                    }
                },
                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = Math.Abs(repairOrderPart.CoreQuantity.GetValueOrDefault().WithDecimalImplied())
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = Math.Abs(repairOrderPart.CoreExtendedPrice.GetValueOrDefault()).WithDecimalImplied()
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = Math.Abs(repairOrderPart.CoreUnitCost.GetValueOrDefault()).WithDecimalImplied()
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(repairOrderPart.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPart.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPart.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPart.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                },
                PartsReturnDestinationCode = new CodeType() { Value = "CORE" }
            };
        }

        private ServicePartsType ForCoreReturn(Part repairOrderPart)
        {
            if (repairOrderPart == null)
            {
                return null;
            }
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = repairOrderPart.Quantity > 0 
                            ? PartConstants.CORE_SALE + " " + GetPartDescription(repairOrderPart)
                            : PartConstants.CORE_RETURN + " " + GetPartDescription(repairOrderPart)
                    }
                },
                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = repairOrderPart.Quantity.GetValueOrDefault().WithDecimalImplied()
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = repairOrderPart.ExtendedPrice.GetValueOrDefault().WithDecimalImplied()
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode,
                                    Value = Math.Abs(repairOrderPart.UnitCost.GetValueOrDefault().WithDecimalImplied())
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(repairOrderPart.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPart.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPart.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPart.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                },
                PartsReturnDestinationCode = new CodeType() { Value = "CORE" }
            };
        }

        private IEnumerable<ServicePartsType> ForExchange(Part repairOrderPart)
        {
            if (repairOrderPart == null)
            {
                return null;
            }
            return new[] {
                ForPartSale(repairOrderPart),
                ForCoreSale(repairOrderPart)
            }.ToArray();
        }

        private IEnumerable<ServicePartsType> ForPartsKit(Part repairOrderPartKit)
        {
            if (repairOrderPartKit == null)
            {
                return null;
            }

            var parts = new List<ServicePartsType> {
                ForPartAssembly(repairOrderPartKit)
            };

            var discountPercentage = ComputeDiscountPercentageForAssembly(repairOrderPartKit);
            var priceAmountApplied = 0.0m;

            foreach (var assemblyPart in repairOrderPartKit.AssemblyParts)
            {
                var discountedPrice = ComputeAssemblyDiscountedPrice(assemblyPart.UnitListPrice, assemblyPart.Quantity, discountPercentage);
                parts.Add(AssemblyPartWithDiscount(repairOrderPartKit, assemblyPart, discountedPrice));
                priceAmountApplied += discountedPrice;
            }

            foreach (var assemblyCharge in repairOrderPartKit?.AssemblyMiscCharges)
            {
                var discountedPrice = ComputeAssemblyDiscountedPrice(assemblyCharge.UnitListPrice, assemblyCharge.Quantity, discountPercentage);
                parts.Add(AssemblyChargeWithDiscount(repairOrderPartKit, assemblyCharge, discountedPrice));
                priceAmountApplied += discountedPrice;
            }

            var remainder = ComputeRemainder(repairOrderPartKit, priceAmountApplied);
            parts.Last().Pricing[0].Price[0].ChargeAmount.Value += remainder.WithDecimalImplied(0);

            return parts;
        }

        private ServicePartsType ForPartAssembly(Part repairOrderPartKit)
        {
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = PartConstants.PART_KIT + " " + GetPartDescription(repairOrderPartKit)
                    }
                },

                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = repairOrderPartKit.Quantity.GetValueOrDefault().WithDecimalImplied()
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = 0m,
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = 0m,
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(repairOrderPartKit.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPartKit.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPartKit.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPartKit.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                }
            };
        }

        private ServicePartsType AssemblyPartWithDiscount(Part repairOrderPartKit, AssemblyPart assemblyPart, decimal discountedPrice)
        {
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = CreateAssemblyPartIdDescription(assemblyPart, repairOrderPartKit)
                    }
                },

                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = CalculateAssemblyQuantity(assemblyPart.Quantity, repairOrderPartKit)
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = CalculateAssemblyExtendedAmount(repairOrderPartKit, discountedPrice),
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = assemblyPart.UnitCost.GetValueOrDefault().WithDecimalImplied(),
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(assemblyPart.PartNumber).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPartKit.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPartKit.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPartKit.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                }
            };
        }

        private ServicePartsType AssemblyChargeWithDiscount(Part repairOrderPartKit, AssemblyMiscCharge assemblyCharge, decimal discountedPrice)
        {
            return new ServicePartsType
            {
                ItemIdDescription = new[] {
                    new TextType {
                        Value = PartConstants.PART_KIT + " " + assemblyCharge.Description
                    }
                },

                ItemQuantity = new QuantityTypeStarQualified
                {
                    Value = CalculateAssemblyQuantity(assemblyCharge.Quantity, repairOrderPartKit)
                },
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = discountedPrice,
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.ExtendedAmount,
                                PriceCodeSpecified = true
                            },
                            new PriceABIETypeStar {
                                ChargeAmount = new AmountType {
                                    Value = assemblyCharge.UnitCost.GetValueOrDefault().WithDecimalImplied(),
                                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                                },
                                PriceCode = PriceEnumeratedType.PartCost,
                                PriceCodeSpecified = true
                            }
                        }
                    }
                },
                ItemIdentificationGroup = new FormattedPartNumber(assemblyCharge.Name).ToItemIdentificationGroup(),
                PartAddedToRODateTime = GetFormattedDateTimeOffset(repairOrderPartKit.AddDate),
                PartAddedToRODateTimeSpecified = repairOrderPartKit.AddDate != null,
                SoldByParty = new PartyABIEType
                {
                    Item = new PersonTypeStar
                    {
                        ID = new[] {
                            new IdentifierType {
                                Value = repairOrderPartKit.AddUsername.MaxLength(SPECIFIED_PERSON_MAX_LENGTH),
                                schemeID = LOCAL
                            }
                        }
                    }
                }
            };
        }

        private static string CreateAssemblyPartIdDescription(AssemblyPart assemblyPart, Part partKit)
        {
            if(assemblyPart.PartType == PartType.CORE)
            {
                return assemblyPart.Quantity * partKit.Quantity > 0 
                    ? PartConstants.PART_KIT + " " + PartConstants.CORE_SALE + " " + GetPartDescription(assemblyPart)
                    : PartConstants.PART_KIT + " " + PartConstants.CORE_RETURN + " "  + GetPartDescription(assemblyPart);
            }
            return PartConstants.PART_KIT + " " + GetPartDescription(assemblyPart);
        }

        private static decimal CalculateAssemblyQuantity(decimal? assemblyQuantity, Part part) =>
            (assemblyQuantity.GetValueOrDefault() * part.Quantity.GetValueOrDefault()).WithDecimalImplied();
        private static decimal CalculateAssemblyExtendedAmount(Part partKit, decimal? discountedPrice) =>
            partKit.Quantity.GetValueOrDefault() * discountedPrice.GetValueOrDefault(); 

        private static decimal ComputeAssemblyDiscountedPrice(decimal? price, decimal? quantity, decimal? discountPercentage) =>
            (price.GetValueOrDefault() * quantity.GetValueOrDefault() * discountPercentage.GetValueOrDefault()).WithDecimalImplied();


        private static decimal? ComputeDiscountPercentageForAssembly(Part partKit)
        {
            var assemblyPartPrice = partKit.AssemblyParts.Sum(part => part.UnitListPrice * part.Quantity);
            var assemblyChargePrice = partKit.AssemblyMiscCharges.Sum(charge => charge.UnitListPrice * charge.Quantity);
            return partKit.UnitPrice / (assemblyPartPrice + assemblyChargePrice);
        }

        private static decimal ComputeRemainder(Part repairOrderPartKit, decimal priceAmountApplied)
        {
            return repairOrderPartKit.UnitPrice.GetValueOrDefault().WithDecimalImplied() - priceAmountApplied;
        }

        private XmlSerializableDateTimeOffset GetFormattedDateTimeOffset(DateTime? date)
        {
            return XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(date, _timeZone);
        }

        private static string GetPartDescription(Part part)
        {
            return String.IsNullOrEmpty(part.Description) ? part.PartNumber : part.Description;
        }
        private static string GetPartDescription(AssemblyPart part)
        {
            return String.IsNullOrEmpty(part.Description) ? part.PartNumber : part.Description;
        }
    }
}
