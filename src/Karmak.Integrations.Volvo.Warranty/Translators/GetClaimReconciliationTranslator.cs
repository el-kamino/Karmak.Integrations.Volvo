using System;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Converters;
using Karmak.Integrations.Volvo.Warranty.Utilities;

namespace Karmak.Integrations.Volvo.Warranty.Translators
{
    public class GetClaimReconciliationTranslator : ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType>
    {
        private readonly string _environment;
        private readonly IDateTimeProvider _dateTimeProvider;

        public GetClaimReconciliationTranslator(string environment, IDateTimeProvider dateTimeProvider)
        {
            _environment = environment;
            _dateTimeProvider = dateTimeProvider;
        }

        public GetServiceProcessingAdvisoryType Translate(GetClaimReconciliationTranslatorArguments args)
        {
            var source = args.Source;
            var settings = args.Settings;

            ValidateArguments(source, settings);

            return new GetServiceProcessingAdvisoryType
            {
                releaseID = ConstantSettings.VolvoOneWarrantySystemVersion,
                systemEnvironmentCode = _environment,
                languageCode = Conversions.StringToLanguageEnumeratedType.Convert(settings.RegionSettings.LanguageCode),
                versionID = settings.DealerServiceProviderSettings.SoftwareVersion,
                ApplicationArea = new ApplicationAreaType
                {
                    CreationDateTime = _dateTimeProvider.Now(),
                    Sender = new SenderType
                    {
                        ComponentID = Conversions.StringToIdentifierType.Convert(settings.DealerServiceProviderSettings.SoftwareName),
                        TaskID = Conversions.StringToIdentifierType.Convert(ConstantSettings.VolvoWarrantyProcessingTaskIdentifier),
                        CreatorNameCode = Conversions.StringToTextType.Convert(settings.DealerServiceProviderSettings.Code),
                        SenderNameCode = Conversions.StringToCodeType.Convert(settings.DealerServiceProviderSettings.ShortCode),
                        DealerNumberID = Conversions.StringToIdentifierType.Convert($"{settings.InterfaceOptions.PaCode}!"),
                        DealerCountryCode = Conversions.StringToCountryEnumeratedType.Convert(settings.RegionSettings.CountryCode),
                        DealerCountryCodeSpecified = true,
                        LanguageCode = settings.RegionSettings.LanguageCode,
                        ServiceID = Conversions.StringToIdentifierType.Convert(ConstantSettings.VolvoClaimReconciliationSenderServiceMessageId)
                    },
                    Destination = new DestinationType
                    {
                        DestinationNameCode = Conversions.StringToCodeType.Convert(ConstantSettings.DestinationNameCode),
                        DestinationSoftwareCode = Conversions.StringToTextType.Convert(ConstantSettings.DestinationSoftwareCode),
                        ServiceMessageID = Conversions.StringToIdentifierType.Convert(ConstantSettings.VolvoClaimReconciliationDestinationServiceMessageId),
                    }
                },
                GetServiceProcessingAdvisoryDataArea = new GetServiceProcessingAdvisoryDataAreaType
                {
                    Get = new GetType
                    {
                        Expression = new[] {
                            new ExpressionType(),
                        }
                    },
                    ServiceProcessingAdvisory = new[] {
                        new ServiceProcessingAdvisoryType {
                            ServiceProcessingAdvisoryHeader = new ServiceProcessingAdvisoryHeaderExtended {
                                DocumentIdentificationGroup = new DocumentIdentificationGroupType {
                                    DocumentIdentification = new DocumentIdentificationType {
                                        DocumentID = Conversions.StringToIdentifierType.Convert(ConstantSettings.Default)
                                    }
                                },
                                PaymentCycleEndDate = source.EndDateTime,
                                PaymentCycleEndDateSpecified = true,
                                DealerParty = new PartyABIEType {
                                    PartyID = new IdentifierType {
                                        Value = $"{settings.InterfaceOptions.PaCode}!"
                                    }
                                },
                                PaymentCycleStartDate = source.StartDateTime,
                                PaymentCycleStartDateSpecified = true
                            }
                        }
                    }
                }
            };
        }

        private void ValidateArguments(ReconciliationFetchRequest source, VolvoSettings settings)
        {
            if (source == null) { throw new ArgumentNullException(nameof(source)); }
            if (settings == null) { throw new ArgumentNullException(nameof(settings)); }
            if (settings.DealerServiceProviderSettings == null) { throw new InvalidOperationException($"{nameof(settings.DealerServiceProviderSettings)} must be defined"); }
            if (settings.InterfaceOptions == null) { throw new InvalidOperationException($"{nameof(settings.InterfaceOptions)} must be defined"); }
            if (settings.RegionSettings == null) { throw new InvalidOperationException($"{nameof(settings.RegionSettings)} must be defined"); }
        }
    }
}
