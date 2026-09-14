using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0
{
    internal class VolvoRepairOrderPartsBuilder
    {
        private decimal? _timeZone;
        private enum VolvoPartType
        {
            Regular,
            Kit,
            Exchange,
            Core,
            CoreReturn,
            AssemblyPart,
            AssemblyCharge
        }

        public VolvoRepairOrderPartsBuilder(decimal? timeZone)
        {
            _timeZone = timeZone;
        }

        public List<VolvoServicePart> BuildServiceParts(RepairOrderTask task)
        {
            var parts = new List<VolvoServicePart>();
            var partsKits = task.Parts.Where(part => part.IsAssemblyPart());
            var exchangeParts = task.Parts.Where(part => part.IsExchangePart());
            var allCoreParts = task.Parts.Where(part => part.IsCorePart(exchangeParts));
            var associatedCores = allCoreParts.Where(core => exchangeParts.Select(part => part.CorePartNumber).Contains(core.PartNumber));
            var unassociatedCores = allCoreParts.Where(core => !exchangeParts.Select(part => part.CorePartNumber).Contains(core.PartNumber));

            var specialPartNumbers = partsKits.Concat(exchangeParts).Concat(allCoreParts).Select(p => p.PartNumber).ToList();
            var regularParts = task.Parts.Where(part => !specialPartNumbers.Contains(part.PartNumber)).ToList();

            foreach (var regularPart in regularParts)
            {
                if (BuildPart(regularPart, VolvoPartType.Regular) is { } part)
                {
                    parts.Add(part);
                }
            }

            foreach (var exchangePart in exchangeParts)
            {
                if (BuildPart(exchangePart, VolvoPartType.Exchange) is { } mainPart)
                {
                    parts.Add(mainPart); //add exchange part
                }

                if (BuildPart(exchangePart, VolvoPartType.Core) is { } corePart)
                {
                    parts.Add(corePart); //add associated core
                }

                //look for associated core - if found, add core sale and core return
                var foundCore = associatedCores.FirstOrDefault(core => core.PartNumber.Equals(exchangePart.CorePartNumber));
                if (foundCore is not null)
                {
                    if (BuildPart(foundCore, VolvoPartType.CoreReturn, exchangePart.CorePartNumber, GetPartDescription(exchangePart)) is { } coreReturnPart)
                    {
                        parts.Add(coreReturnPart); //add returned core
                    }

                }
            }

            foreach (var corePart in unassociatedCores)
            {
                if (BuildPart(corePart, VolvoPartType.CoreReturn) is { } coreReturnPart)
                {
                    parts.Add(coreReturnPart); //add any cores that aren't associated with an exchange part
                }
            }

            foreach (var partKit in partsKits)
            {
                if (BuildPart(partKit, VolvoPartType.Kit) is { } part)
                {
                    //adds the KitPart with no pricing, then the individual assembly parts and charges with discounted pricing 
                    parts.Add(part);

                    var discountPercentage = ComputeDiscountPercentageForAssembly(partKit);
                    var priceAmountAppliedWithDecimalImplied = 0.0m;

                    foreach (var assemblyPart in partKit.AssemblyParts)
                    {
                        var discountedPrice = ComputeAssemblyDiscountedPrice(assemblyPart.UnitListPrice, assemblyPart.Quantity, discountPercentage);
                        parts.Add(BuildAssemblyPart(assemblyPart, partKit, discountedPrice));
                        priceAmountAppliedWithDecimalImplied += discountedPrice.WithDecimalImplied();
                    }

                    foreach (var assemblyCharge in partKit?.AssemblyMiscCharges)
                    {
                        var discountedPrice = ComputeAssemblyDiscountedPrice(assemblyCharge.UnitListPrice, assemblyCharge.Quantity, discountPercentage);
                        parts.Add(BuildAssemblyCharge(assemblyCharge, partKit, discountedPrice));
                        priceAmountAppliedWithDecimalImplied += discountedPrice.WithDecimalImplied();
                    }

                    //We split discounts across all items. Due to rounding, there may be a small remainder amount that gets left off,
                    //  so we apply that remainder to the last item in the kit to ensure the total price is correct.
                    var remainder = partKit.UnitPrice.GetValueOrDefault().WithDecimalImplied() - priceAmountAppliedWithDecimalImplied;
                    parts.Last().price.extendedAmount += remainder.WithDecimalImplied(0);
                }
            }
            return parts;
        }

        private VolvoServicePart BuildPart(Part part, VolvoPartType partType, string overridePartNumber = "", string overrideDescription = "")
        {
            if (part == null) return null;

            //There are some very funky rules in existing 5.2 functionality regarding what gets an Absolute value and what doesn't,
            //Putting this all in one place and following existing 5.2 functionality to a T to avoid any introduced issues.
            //See inline comments for details on the funky rules.

            var partNumber = String.IsNullOrEmpty(overridePartNumber) ? part.PartNumber : overridePartNumber;
            var partDescription = String.IsNullOrEmpty(overrideDescription) ? GetPartDescription(part) : overrideDescription;

            var servicePart = new VolvoServicePart()
            {
                //default properties that may be overwritten later by funky rules
                itemIdDescription = partDescription.WafSanitize().MaxLength(50),
                itemQuantity = part.Quantity.GetValueOrDefault().WithDecimalImplied(),
                price = BuildPrice(
                            part.UnitCost,
                            part.ExtendedPrice
                        ),

                //common properties for all part types
                partsId = BuildPartsId(partNumber.RemoveSpecialCharacters().MaxLength(22)),
                partAddedToRODateTime = DateTimeUtility.ApplyCustomTimeZone(part.AddDate.GetValueOrDefault(), _timeZone),
                soldByParty = new List<VolvoPartyIdentifier>
                {
                    new() {
                        id = part.AddUsername.WafSanitize().MaxLength(100),
                        type = CustomerTypeCodes.PartyIdentifierLocal
                    }
                }
            };

            switch (partType)
            {
                case VolvoPartType.Regular:
                    // force a negative extended price when quantity is negative (I think this is moot)
                    servicePart.price.extendedAmount = part.Quantity < 0 ?
                            -Math.Abs(servicePart.price.extendedAmount.GetValueOrDefault())
                            : Math.Abs(servicePart.price.extendedAmount.GetValueOrDefault());
                    break;
                case VolvoPartType.Exchange:
                    // absolute values for quantity and extended amount
                    servicePart.itemQuantity = Math.Abs(servicePart.itemQuantity.GetValueOrDefault());
                    servicePart.price.extendedAmount = Math.Abs(servicePart.price.extendedAmount.GetValueOrDefault());
                    break;
                case VolvoPartType.Core:
                    //prepend itemIdDescription, add partsReturnDestinationCode
                    servicePart.itemIdDescription = $"{PartConstants.CORE_SALE} {servicePart.itemIdDescription}".MaxLength(50);
                    servicePart.partsReturnDestinationCode = "CORE";
                    //absolute values for quantity, extended amount, and part cost
                    servicePart.itemQuantity = Math.Abs(servicePart.itemQuantity.GetValueOrDefault());
                    servicePart.price.extendedAmount = Math.Abs(part.CoreExtendedPrice.GetValueOrDefault().WithDecimalImplied());
                    servicePart.price.partCost = Math.Abs(part.CoreUnitCost.GetValueOrDefault().WithDecimalImplied());
                    break;
                case VolvoPartType.CoreReturn:
                    //prepend itemIdDescription, add partsReturnDestinationCode
                    servicePart.itemIdDescription = part.Quantity > 0 ? $"{PartConstants.CORE_SALE} {servicePart.itemIdDescription}".MaxLength(50)
                        : $"{PartConstants.CORE_RETURN} {servicePart.itemIdDescription}".MaxLength(50);
                    servicePart.partsReturnDestinationCode = "CORE";
                    //absolute value for part cost
                    servicePart.price.partCost = Math.Abs(servicePart.price.partCost.GetValueOrDefault());
                    break;
                case VolvoPartType.Kit:
                    //prepend itemIdDescription
                    servicePart.itemIdDescription = $"{PartConstants.PART_KIT} {servicePart.itemIdDescription}".WafSanitize().MaxLength(50);
                    //zero out extended amount and part cost
                    servicePart.price.extendedAmount = 0m;
                    servicePart.price.partCost = 0m;
                    break;
            }

            return servicePart;
        }

        private VolvoServicePart BuildAssemblyPart(AssemblyPart assemblyPart, Part partKit, decimal? discountedPrice)
        {
            return new VolvoServicePart()
            {
                itemIdDescription = GetPartDescription(assemblyPart, partKit).WafSanitize().MaxLength(50),
                itemQuantity = (assemblyPart.Quantity.GetValueOrDefault() * partKit.Quantity.GetValueOrDefault()).WithDecimalImplied(),
                price = BuildPrice(
                           assemblyPart.UnitCost,
                           partKit.Quantity.GetValueOrDefault() * discountedPrice
                       ),
                partsId = BuildPartsId(assemblyPart.PartNumber.RemoveSpecialCharacters().MaxLength(22)),
                partAddedToRODateTime = DateTimeUtility.ApplyCustomTimeZone(partKit.AddDate.GetValueOrDefault(), _timeZone),
                soldByParty = new List<VolvoPartyIdentifier>
                {
                    new VolvoPartyIdentifier
                    {
                        id = partKit.AddUsername.WafSanitize().MaxLength(100),
                        type = CustomerTypeCodes.PartyIdentifierLocal
                    }
                }
            };
        }

        private VolvoServicePart BuildAssemblyCharge(AssemblyMiscCharge assemblyCharge, Part partKit, decimal? discountedPrice)
        {
            return new VolvoServicePart()
            {
                itemIdDescription = (PartConstants.PART_KIT + " " + assemblyCharge.Description).WafSanitize().MaxLength(50),
                itemQuantity = (assemblyCharge.Quantity.GetValueOrDefault() * partKit.Quantity.GetValueOrDefault()).WithDecimalImplied(),
                price = BuildPrice(
                           assemblyCharge.UnitCost,
                           discountedPrice
                       ),
                partsId = BuildPartsId(assemblyCharge.Name.RemoveSpecialCharacters().MaxLength(22)),
                partAddedToRODateTime = DateTimeUtility.ApplyCustomTimeZone(partKit.AddDate.GetValueOrDefault(), _timeZone),
                soldByParty = new List<VolvoPartyIdentifier>
                {
                    new() {
                        id = partKit.AddUsername.WafSanitize().MaxLength(100),
                        type = CustomerTypeCodes.PartyIdentifierLocal
                    }
                }
            };
        }


        private static string GetPartDescription(Part part)
        {
            return String.IsNullOrEmpty(part.Description) ? part.PartNumber : part.Description;
        }

        private static string GetPartDescription(AssemblyPart assemblyPart, Part partKit)
        {
            var description = String.IsNullOrEmpty(assemblyPart.Description) ? assemblyPart.PartNumber : assemblyPart.Description;
            if (assemblyPart.PartType == PartType.CORE)
            {
                return assemblyPart.Quantity * partKit.Quantity > 0
                    ? $"{PartConstants.PART_KIT} {PartConstants.CORE_SALE} {description}"
                    : $"{PartConstants.PART_KIT} {PartConstants.CORE_RETURN} {description}";
            }
            return $"{PartConstants.PART_KIT} {description}";
        }

        private static VolvoServicePartPrice BuildPrice(decimal? unitPrice, decimal? extendedPrice)
        {
            return new VolvoServicePartPrice()
            {
                partCost = unitPrice.GetValueOrDefault().WithDecimalImplied(),
                extendedAmount = extendedPrice.GetValueOrDefault().WithDecimalImplied()
            };
        }

        private static List<VolvoPartsIdentifier> BuildPartsId(string partNumber)
        {
            if (String.IsNullOrEmpty(partNumber))
                return null;

            //Volvo has a very funky way of breaking up part numbers.  prefix 6 + base 8 + suffix 8.
            //So we need to break up the part number into those chunks and send as a list of parts identifiers.
            var partIds = new List<VolvoPartsIdentifier>
            {
                new VolvoPartsIdentifier { itemId = partNumber.Substring(0, Math.Min(6, partNumber.Length)), type = "Part Prefix" }
            };

            if (partNumber.Length > 6)
                partIds.Add(new VolvoPartsIdentifier { itemId = partNumber.Substring(6, Math.Min(8, partNumber.Length - 6)), type = "Part Base" });

            if (partNumber.Length > 14)
                partIds.Add(new VolvoPartsIdentifier { itemId = partNumber.Substring(14), type = "Part Suffix" });

            return partIds;
        }


        #region Assembly Part Discount Calculation
        private static decimal? ComputeDiscountPercentageForAssembly(Part partKit)
        {
            var assemblyPartPrice = partKit.AssemblyParts.Sum(part => part.UnitListPrice * part.Quantity);
            var assemblyChargePrice = partKit.AssemblyMiscCharges.Sum(charge => charge.UnitListPrice * charge.Quantity);
            return partKit.UnitPrice / (assemblyPartPrice + assemblyChargePrice);
        }
        private static decimal ComputeAssemblyDiscountedPrice(decimal? price, decimal? quantity, decimal? discountPercentage) =>
            (price.GetValueOrDefault() * quantity.GetValueOrDefault() * discountPercentage.GetValueOrDefault());
        #endregion
    }
}
