using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;
using WarrantyDiagnostics = Karmak.Integrations.Volvo.Warranty.Contracts.Diagnostics;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public abstract class SubmitClaimJobTranslator : ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended>
    {
        public abstract JobExtended Translate(SubmitClaimJobTranslatorArguments args);

        protected static string GetClaimType(IClaim source) => source.InAppeal ? ClaimTypes.Appeal : source.Type;

        protected static WarrantyClaimExtended MapWarrantyClaim(IClaim source, Action<WarrantyClaimExtended> action = null)
        {
            var claim = new WarrantyClaimExtended
            {
                ClaimTypeString = source.SubCode,
                ApprovalCode = Conversions.StringToCodeTypes.Convert(source.RepairOrder.ApprovalIdentifiers),
                ManualReviewCode = Conversions.StringToCodeType.Convert(source.IsManualReviewRequired.ToString())
            };

            if (source.InAppeal)
            {
                claim.AppealComments = Conversions.StringToTextType.Convert(source.RepairOrder.Jobs?.FirstOrDefault()?.AppealComments);
                claim.AppealReasonCode = Conversions.StringToCodeType.Convert(source.RepairOrder.Jobs?.FirstOrDefault()?.AppealReasonCode);
            }

            action?.Invoke(claim);

            return claim;
        }

        protected static DiagnosticsExtended MapDiagnostics(WarrantyDiagnostics diagnostics) =>
            new DiagnosticsExtended
            {
                DiagnosticCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.DiagnosticTroubleCodes),
                EngineLightIndicator = diagnostics?.IsCheckEngineLightOn ?? default,
                EngineLightIndicatorSpecified = diagnostics != null,
                ReqMeasurementOrResults = diagnostics?.PolicyRequiredMeasurementOrResults?.ToArray(),
                BatteryCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.BatteryCodes),
                BodyCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.BodyCodes),
                ChassisCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.ChassisCodes),
                UndefinedDiagnosticCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.UndefinedDiagnosticCodes),
                KOEOCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.KeyOnEngineOffCodes),
                KOECCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.KeyOnEngineColdCodes),
                KOERCodes = Conversions.StringToCodeTypes.Convert(diagnostics?.KeyOnEngineRunningCodes),
                ReplacedTireDOTCode = Conversions.StringToCodeTypes.Convert(diagnostics?.ReplacedTireDepartmentOfTransportationCodes),
                ReplacementTireDOTCode = Conversions.StringToCodeTypes.Convert(diagnostics?.ReplacementTireDepartmentOfTransportationCodes),
            };

        protected static Func<PartExpense, ServicePartsExtended> MapServiceParts(IClaim source, IConvertible<decimal, AmountType> decimalConverter, bool includeFleet = false) =>
            partExpense =>
            {
                var servicePart = new ServicePartsExtended
                {
                    ItemIdentificationGroup =
                        new[] {
                            new ItemIdentificationType {
                                ItemID = Conversions.StringToIdentifierType.Convert($"{partExpense.Prefix}!{partExpense.Number}!{partExpense.Suffix}")
                            }
                        },
                    OutsidePartsInvoiceNumber = partExpense.ThirdPartyInvoiceIdentifier,
                    CodesAndCommentsExpanded = Conversions.GetOrNull(partExpense, p => p.IsCausalPart, p =>
                        new CodesAndCommentsExpandedType1
                        {
                            CausalPartIndicator = Conversions.StringToIdentifierType.Convert(p.IsCausalPart.ToString()),
                            ConditionCode = Conversions.StringToCodeType.Convert(p.ConditionCode)
                        }),
                    ItemQuantity = Conversions.GetOrNull(partExpense.Quantity, quantity => Conversions.DecimalToQuantityType.Convert(quantity.Value)),
                    Pricing = new[] {
                        Conversions.GetOrNull(partExpense.CorePrice,
                            corePrice => new PricingABIEType {
                                Price = new[] {
                                    new PriceABIEType {
                                        ChargeAmount = decimalConverter.Convert(corePrice.Value),
                                        PriceDescription = new[] {
                                            Conversions.PriceComponentTypeToDescriptionTextType.Convert(PricingComponentType.Core)
                                        }
                                    }
                                }
                            }),
                        Conversions.GetOrNull(partExpense.ExtendedPrice,
                            extendedPrice => new PricingABIEType {
                                Price = new[] {
                                    new PriceABIEType {
                                        ChargeAmount = decimalConverter.Convert(extendedPrice.Value),
                                        PriceDescription = new[] {
                                            Conversions.PriceComponentTypeToDescriptionTextType.Convert(PricingComponentType.Extended)
                                        }
                                    }
                                }
                            })
                    }
                };

                if (includeFleet)
                {
                    servicePart.FleetDiscountPercent = source.FleetAccount?.PartsDiscountPercentage ?? default;
                    servicePart.FleetDiscountPercentSpecified = source.FleetAccount?.PartsDiscountPercentage != null;
                }

                if (source.InAppeal)
                {
                    servicePart.AppealActionCode = Conversions.StringToCodeType.Convert(partExpense.AppealCode);
                }

                return servicePart;
            };

        protected static Func<LaborExpense, ServiceLaborExtended> MapServiceLabor(IClaim source) =>
            laborExpense =>
            {
                var serviceLabor = new ServiceLaborExtended
                {
                    LaborOperationID = Conversions.StringToIdentifierType.Convert(laborExpense.Identifier),
                    LaborActualHoursNumeric = laborExpense.Quantity?.Type == UnitOfMeasureType.Hours ? laborExpense.Quantity.Value : default,
                    LaborActualHoursNumericSpecified = laborExpense.Quantity?.Type == UnitOfMeasureType.Hours,
                    ServiceTechnicianParty =
                        Conversions.GetOrNull(laborExpense.Technician?.Identifier,
                            identifier => new[] {
                                new PartyABIEType {PartyID = Conversions.StringToIdentifierType.Convert(identifier),}
                            }),
                    Sublet = new[] {
                        new SubletType {
                            Pricing = new[] {
                                new PricingABIEType {
                                    Price = new[] {
                                        new PriceABIEType()
                                    }
                                }
                            },
                            SubletInvoiceNumberString = laborExpense.ThirdPartyInvoiceIdentifier,
                            SubletCode = Conversions.StringToCodeType.Convert(ConstantSettings.Default),
                            SubletWorkDescription = new[] {
                                Conversions.StringToTextType.Convert(ConstantSettings.Default)
                            }
                        }
                    }
                };

                if (source.InAppeal)
                {
                    serviceLabor.AppealActionCode = Conversions.StringToCodeType.Convert(laborExpense.AppealCode);
                }

                return serviceLabor;
            };

        protected static PricingABIEType MapServiceLaborPricing(IEnumerable<LaborExpense> laborExpenses, IConvertible<decimal, AmountType> moneyConverter) =>
            new PricingABIEType
            {
                Price = new[] {
                    new PriceABIEType {
                        ChargeAmount = moneyConverter.Convert(laborExpenses?.Sum(expense => expense.Total?.Value) ?? default),
                        PriceDescription = new[] {
                            Conversions.StringToTextType.Convert(ConstantSettings.LaborPricingDescription)
                        }
                    }
                }
            };

        protected static Func<MiscellaneousExpense, ServiceComponentsExtended> MapServiceComponent(IClaim source, IConvertible<Money, AmountType> moneyConverter) =>
            miscellaneousExpense =>
            {
                var daysExpensed = miscellaneousExpense.Quantity?.Type == UnitOfMeasureType.Days && miscellaneousExpense.Quantity?.Value != 0;
                var hoursExpensed = miscellaneousExpense.SecondQuantity?.Type == UnitOfMeasureType.Hours && miscellaneousExpense.SecondQuantity?.Value != 0 && !daysExpensed;
                var serviceComponent = new ServiceComponentsExtended
                {
                    DocumentID = Conversions.StringToIdentifierType.Convert(miscellaneousExpense.ThirdPartyInvoiceIdentifier),
                    ServiceCode = Conversions.StringToCodeType.Convert(miscellaneousExpense.Identifier),
                    ExpenseDaysNumeric = daysExpensed ? miscellaneousExpense.Quantity.Value : default,
                    ExpenseDaysNumericSpecified = daysExpensed,
                    ExpenseHoursNumeric = hoursExpensed ? miscellaneousExpense.SecondQuantity.Value : default,
                    ExpenseHoursNumericSpecified = hoursExpensed,
                    Pricing = new[] {
                        new PricingABIEType {
                            Price = new[] {
                                new PriceABIEType {
                                    ChargeAmount = moneyConverter.Convert(miscellaneousExpense.Total)
                                }
                            }
                        }
                    }
                };

                if (source.InAppeal)
                {
                    serviceComponent.AppealActionCode = Conversions.StringToCodeType.Convert(miscellaneousExpense.AppealCode);
                }

                return serviceComponent;
            };

        protected static PartyABIEType[] EmptyServicePartyTechnician() => new[] { new PartyABIEType() };
    }
}
