using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Converters;

namespace Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim
{
    public class SubmitClaimTranslator : ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType>
    {
        private readonly ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended> _translator;
        private readonly string _environmentName;

        public SubmitClaimTranslator(ITranslatable<SubmitClaimJobTranslatorArguments, JobExtended> translator, string environmentName)
        {
            _translator = translator;
            _environmentName = environmentName;
        }

        public ProcessRepairOrderType Translate(SubmitClaimTranslatorArguments args)
        {
            var source = args.Source;
            var settings = args.Settings;

            ValidateClaimAndSettings(source, settings);

            var includeFleet = source.Any(claim => claim.Type.Equals(ClaimTypes.Fleet));
            var isExtendedService = source.Any(claim => claim.Type.Equals(ClaimTypes.ExtendedServiceContracts));

            return new ProcessRepairOrderType
            {
                releaseID = ConstantSettings.VolvoOneWarrantySystemVersion,
                systemEnvironmentCode = _environmentName,
                ApplicationArea = MapApplicationArea(settings),
                ProcessRepairOrderDataArea = new ProcessRepairOrderDataAreaType
                {
                    Process = Conversions.StringToProcessType.Convert(ConstantSettings.Never),
                    RepairOrder = Conversions.GetOrNull(source.First().RepairOrder, repairOrder => new[] {
                        new RepairOrderType {
                            RepairOrderHeader = MapRepairOrderHeader(source.First(), repairOrder, settings, includeFleet, isExtendedService),
                            Job = MapRepairOrderJobs(source, GetCurrencyCode(settings))
                        }
                    })
                }
            };
        }

        private JobExtended[] MapRepairOrderJobs(IEnumerable<IClaim> source, CurrencyCode dealerRegionCurrency)
        {
            var claims = source.OrderBy(x => x.RepairOrder.Jobs?.FirstOrDefault()?.Identifier);
            var args = new SubmitClaimJobTranslatorArguments()
            {
                DealerRegionCurrency = dealerRegionCurrency
            };

            var jobs = new List<JobExtended>();
            foreach (var claim in claims)
            {
                args.Source = claim;
                jobs.Add(_translator.Translate(args));
            }

            return jobs.ToArray();
        }

        protected static void ValidateClaimAndSettings(IEnumerable<IClaim> source, VolvoSettings settings)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (settings.DealerServiceProviderSettings == null) { throw new ArgumentNullException(nameof(settings), "VolvoSettings.DealerServiceProviderSettings must be configured"); }
            if (settings.InterfaceOptions == null) { throw new ArgumentNullException(nameof(settings), "VolvoSettings.InterfaceOptions must be configured"); }
            if (settings.RegionSettings == null) { throw new ArgumentNullException(nameof(settings), "VolvoSettings.RegionSettings must be configured"); }
        }

        protected static CurrencyCode GetCurrencyCode(VolvoSettings settings) =>
            Enum.TryParse(settings.RegionSettings.CurrencyCode, out CurrencyCode currencyCode)
                ? currencyCode
                : ConstantSettings.CurrencyCodeDefault;

        protected static ApplicationAreaType MapApplicationArea(VolvoSettings settings) =>
            new ApplicationAreaType
            {
                CreationDateTime = DateTime.UtcNow,
                Destination = new DestinationType
                {
                    DestinationNameCode = Conversions.StringToCodeType.Convert(ConstantSettings.DestinationNameCode),
                    DestinationSoftwareCode = Conversions.StringToTextType.Convert(ConstantSettings.DestinationSoftwareCode),
                    ServiceMessageID = Conversions.StringToIdentifierType.Convert(ConstantSettings.DestinationServiceMessageId),
                    DestinationSoftware = Conversions.StringToTextType.Convert(ConstantSettings.DestinationSoftwareVersion),
                },
                Sender = new SenderType
                {
                    ComponentID = Conversions.StringToIdentifierType.Convert(settings.DealerServiceProviderSettings.SoftwareName),
                    ConfirmationCode = ConstantSettings.ConfirmationCode,
                    ConfirmationCodeSpecified = true,
                    SystemVersion = settings.DealerServiceProviderSettings.SoftwareVersion,
                    CreatorNameCode = Conversions.StringToTextType.Convert(settings.DealerServiceProviderSettings.Code),
                    SenderNameCode = Conversions.StringToCodeType.Convert(settings.DealerServiceProviderSettings.ShortCode),
                    TaskID = Conversions.StringToIdentifierType.Convert(ConstantSettings.RepairOrderTaskIdentifier),
                    DealerNumberID = Conversions.StringToIdentifierType.Convert($"{settings.InterfaceOptions.PaCode}!"),
                    DealerCountryCode = Conversions.StringToCountryEnumeratedType.Convert(settings.RegionSettings.CountryCode),
                    DealerCountryCodeSpecified = true,
                    LanguageCode = settings.RegionSettings.LanguageCode
                }
            };

        protected static RepairOrderHeaderType MapRepairOrderHeader(IClaim source, RepairOrder repairOrder, VolvoSettings settings, bool includeFleet = false, bool isExtendedService = false)
        {
            var lineItem = new RepairOrderVehicleLineItemExtended
            {
                Vehicle = new VehicleABIEType
                {
                    VehicleID = Conversions.StringToIdentifierType.Convert(source.Unit?.Vehicle?.Identifier),
                    VehicleNote = new[] {
                        Conversions.StringToTextType.Convert(source.Unit?.Vehicle?.SpecialUseIdentifier)
                    },
                    Engine = new EngineType
                    {
                        TotalEngineHoursNumeric = source.Unit?.GetReading(MeterReadingType.EngineHours)?.ReadingIn?.Value ?? default,
                        TotalEngineHoursNumericSpecified = source.Unit?.GetReading(MeterReadingType.EngineHours)?.ReadingIn?.Value != null
                    }
                },
                LicenseNumberString = source.Unit?.Vehicle?.License?.Number,
                LicenseGeoLocationString = source.Unit?.Vehicle?.License?.State
            };

            if (includeFleet)
            {
                lineItem.FleetAccount = Conversions.GetOrNull(source.FleetAccount, fleetAccount => new FleetAccountType
                {
                    FleetAccountString = fleetAccount.Identifier,
                    FleetPurchaseOrderNumberString = fleetAccount.PurchaseOrderIdentifier
                });
            }

            var header = new RepairOrderHeaderType
            {
                DocumentIdentificationGroup = new DocumentIdentificationGroupType
                {
                    DocumentIdentification = new DocumentIdentificationType
                    {
                        DocumentID = HasApprovalIdentifiers(repairOrder) ?
                            Conversions.StringToIdentifierType.Convert(repairOrder.Identifier)
                            : Conversions.StringToIdentifierType.Convert(repairOrder.SecondaryIdentifier)
                    }
                },
                DealerParty = new PartyABIEType
                {
                    PartyID = Conversions.StringToIdentifierType.Convert($"{settings.InterfaceOptions.PaCode}!"),
                    LocationID = Conversions.StringToIdentifierType.Convert(settings.RegionSettings.CountryCode)
                },
                PrimaryDriver = Conversions.GetOrNull(source.Driver, d => new PrimaryDriverType
                {
                    DriverParty = new PartyABIEType
                    {
                        PartyID = Conversions.StringToIdentifierType.Convert(d.Identifier),
                        Item = new PersonType
                        {
                            GivenName = new[] {
                                Conversions.StringToNameType.Convert(d.FullName)
                            }
                        }
                    }
                }),
                RepairOrderVehicleLineItem = lineItem,
                RepairOrderOpenedDate = repairOrder.OpenedDate.GetValueOrDefault(DateTime.MinValue),
                RepairOrderOpenedDateSpecified = repairOrder.OpenedDate.HasValue,
                RepairOrderCompletedDate = repairOrder.CompletedDate.GetValueOrDefault(DateTime.MinValue),
                ServiceAdvisorParty = Conversions.GetOrNull(repairOrder.Advisor?.Identifier,
                    identifier =>
                        new PartyABIEType
                        {
                            PartyID = Conversions.StringToIdentifierType.Convert(identifier)
                        }),
                InDistanceMeasure = Conversions.MeasurementToMeasurementLengthType.Convert(source.Unit?.GetReading(MeterReadingType.Odometer)?.ReadingIn)
            };

            if (isExtendedService)
            {
                header.ESCFranchiseIndicator = repairOrder.IsExtendedServiceContractFranchise;
                header.ESCFranchiseIndicatorSpecified = repairOrder.IsExtendedServiceContractFranchise;
            }

            return header;
        }

        private static bool HasApprovalIdentifiers(RepairOrder repairOrder)
        {
            return repairOrder.ApprovalIdentifiers?.Any(x => !string.IsNullOrEmpty(x)) ?? false;
        }
    }
}
