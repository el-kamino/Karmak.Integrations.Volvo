using System;
using System.Linq;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class OverTheCounterClaimTranslator : SubmitClaimJobTranslator
    {
        public OverTheCounterClaimTranslator() { }

        public override JobExtended Translate(SubmitClaimJobTranslatorArguments args)
        {
            IClaim claim = args.Source;
            CurrencyCode dealerRegionCurrency = args.DealerRegionCurrency;
            var job = claim.RepairOrder.Jobs?.FirstOrDefault();

            return new JobExtended
            {
                JobTypeString = GetClaimType(claim),
                JobNumberString = job?.Identifier,
                OperationID = Conversions.StringToIdentifierType.Convert(ConstantSettings.Default),
                OperationName = Conversions.StringToTextType.Convert(ConstantSettings.Default),
                HoldAtPreValidationIndicator = claim.ShouldHoldAtPreValidation ?? default,
                HoldAtPreValidationIndicatorSpecified = claim.ShouldHoldAtPreValidation != null,
                JobCompletionDate = claim.RepairOrder.CompletedDate.GetValueOrDefault(DateTime.MinValue),
                JobCompletionDateSpecified = claim.RepairOrder.CompletedDate.HasValue,
                PreDefinedRepairCode = Conversions.StringToCodeType.Convert(claim.PreAuthorizationIdentifier),
                OutDistanceMeasure = Conversions.MeasurementToMeasurementLengthType.Convert(claim.Unit?.GetReading(MeterReadingType.Odometer)?.ReadingOut),
                OutEngOperatingHoursNumeric = claim.Unit?.GetReading(MeterReadingType.EngineHours)?.ReadingOut?.Value ?? default,
                OutEngOperatingHoursNumericSpecified = claim.Unit?.GetReading(MeterReadingType.EngineHours)?.ReadingOut?.Value != null,
                WarrantyClaim = new[] {
                    MapWarrantyClaim(claim, warrantyClaim => {
                        warrantyClaim.OriginalRepairOrderNumberString = job?.PreviousPartClaim?.InvoiceIdentifier;
                        warrantyClaim.SparePartDate = job?.PreviousPartClaim?.InvoiceDate ?? default;
                        warrantyClaim.SparePartDateSpecified = job?.PreviousPartClaim?.InvoiceDate != null;
                        warrantyClaim.SparePartDistanceMeasure = Conversions.MeasurementToMeasurementLengthType.Convert(job?.PreviousPartClaim?.Measurement);
                    })
                },
                CodesAndCommentsExpanded = new CodesAndCommentsExpandedType
                {
                    ComplaintCode = Conversions.StringToCodeType.Convert(job?.ComplaintCode),
                    ComplaintDescription = new[] {
                        Conversions.StringToTextType.Convert(job?.CustomerNotes)
                    },
                    TechnicianNotes = Conversions.StringToTextType.Convert(job?.TechnicianNotes)
                },
                Diagnostics = new[] {
                    MapDiagnostics(job?.Diagnostics)
                },
                ServiceParts = job?.PartExpenses?
                    .Select(MapServiceParts(claim, Conversions.DecimalToAmountType(dealerRegionCurrency)))
                    .ToArray(),
                ServiceLabor = job?.LaborExpenses?
                    .Select(MapServiceLabor(claim))
                    .ToArray(),
                Pricing = new[] {
                    MapServiceLaborPricing(job?.LaborExpenses, Conversions.DecimalToAmountType(dealerRegionCurrency))
                },
                ServiceComponents = job?.MiscellaneousExpenses?
                    .Select(MapServiceComponent(claim, Conversions.MoneyToAmountType(dealerRegionCurrency)))
                    .ToArray(),
                ServiceTechnicianParty = EmptyServicePartyTechnician()
            };
        }
    }
}
