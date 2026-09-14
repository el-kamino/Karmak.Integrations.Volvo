using System.Xml.Linq;
using AutoBogus;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils.Xml.Testing;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Utilities;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Translators
{
    public class GetClaimReconciliationTranslatesToVolvoBodXmlTest
    {
        private readonly VolvoSettings _fakeSettings;
        private readonly GetClaimReconciliationTranslator _getClaimReconciliationTranslator;
        private readonly string _environment = "Test";
        private readonly DateTime _today;
        private readonly XNamespace _starNamespace = "http://www.starstandard.org/STAR/5";
        private readonly XNamespace _owsNamespace = "urn:volvo/WarrantyClaim/Extended/ServiceProcessingAdvisory/v1.0";
        private readonly XNamespace _oagisNamespace = "http://www.openapplications.org/oagis/9";
        private ReconciliationFetchRequest _claimReconciliationRequest;

        public GetClaimReconciliationTranslatesToVolvoBodXmlTest()
        {
            _today = new DateTime(2019, 1, 1, 0, 0, 0);
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            dateTimeProvider.Now().Returns(_today);

            _claimReconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            _getClaimReconciliationTranslator =
                new GetClaimReconciliationTranslator(_environment, dateTimeProvider);

            _fakeSettings = new VolvoSettings
            {
                DealerServiceProviderSettings =
                    new DealerServiceProviderSettings
                    {
                        Code = "Karmak",
                        ShortCode = "KM",
                        SoftwareName = "Fusion",
                        SoftwareVersion = "1.0.0"
                    },
                RegionSettings = new RegionSettings { LanguageCode = "en-US", CountryCode = "CA", CurrencyCode = "USD" },
                InterfaceOptions = new InterfaceOptions
                {
                    PaCode = "K1234",
                    OemUserMappings = new Dictionary<string, string> { ["tnugent"] = "010203" }
                }
            };
        }

        [Fact]
        public void WhenTranslatingWithNullSettings_It_Throws()
        {
            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = new ReconciliationFetchRequest
                {
                    StartDateTime = new DateTime(2020, 2, 1),
                    EndDateTime = new DateTime(2020, 2, 2)
                },
                Settings = null
            };

            Assert.Throws<ArgumentNullException>(() =>
                _getClaimReconciliationTranslator.Translate(args));
        }

        [Fact]
        public void WhenTranslatingWithNullDealerServiceProviderSettings_It_Throws()
        {
            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = new ReconciliationFetchRequest
                {
                    StartDateTime = new DateTime(2020, 2, 1),
                    EndDateTime = new DateTime(2020, 2, 2)
                },
                Settings = new VolvoSettings
                {
                    DealerServiceProviderSettings = null,
                    InterfaceOptions = AutoFaker.Generate<InterfaceOptions>(),
                    RegionSettings = AutoFaker.Generate<RegionSettings>()
                }
            };

            Assert.Throws<InvalidOperationException>(() =>
                _getClaimReconciliationTranslator.Translate(args));
        }

        [Fact]
        public void WhenTranslatingWithNullInterfaceOptions_It_Throws()
        {
            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = new ReconciliationFetchRequest
                {
                    StartDateTime = new DateTime(2020, 2, 1),
                    EndDateTime = new DateTime(2020, 2, 2)
                },
                Settings = new VolvoSettings
                {
                    DealerServiceProviderSettings = AutoFaker.Generate<DealerServiceProviderSettings>(),
                    InterfaceOptions = null,
                    RegionSettings = AutoFaker.Generate<RegionSettings>()
                }
            };

            Assert.Throws<InvalidOperationException>(() => _getClaimReconciliationTranslator.Translate(args));
        }

        [Fact]
        public void WhenTranslatingWithNullRegionSettings_It_Throws()
        {
            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = new ReconciliationFetchRequest
                {
                    StartDateTime = new DateTime(2020, 2, 1),
                    EndDateTime = new DateTime(2020, 2, 2)
                },
                Settings = new VolvoSettings
                {
                    DealerServiceProviderSettings = AutoFaker.Generate<DealerServiceProviderSettings>(),
                    InterfaceOptions = AutoFaker.Generate<InterfaceOptions>(),
                    RegionSettings = null
                }
            };

            Assert.Throws<InvalidOperationException>(() => _getClaimReconciliationTranslator.Translate(args));
        }

        [Fact]
        public void WhenTranslatingWithNullSource_It_Throws()
        {
            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = null,
                Settings = new VolvoSettings
                {
                    DealerServiceProviderSettings = AutoFaker.Generate<DealerServiceProviderSettings>(),
                    InterfaceOptions = AutoFaker.Generate<InterfaceOptions>(),
                    RegionSettings = AutoFaker.Generate<RegionSettings>()
                }
            };

            Assert.Throws<ArgumentNullException>(() => _getClaimReconciliationTranslator.Translate(args));
        }

        [Fact]
        public void WhenTranslatingSuccessfully_It_ReturnsAGetClaim()
        {
            var claimReconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            var args = new GetClaimReconciliationTranslatorArguments()
            {
                Source = claimReconciliationRequest,
                Settings = _fakeSettings
            };

            var getClaimReconciliationRequest = _getClaimReconciliationTranslator.Translate(args);

            Assert.NotNull(getClaimReconciliationRequest);
            Assert.Equal(_today, getClaimReconciliationRequest.ApplicationArea.CreationDateTime);
            Assert.Equal(ConstantSettings.VolvoOneWarrantySystemVersion, getClaimReconciliationRequest.releaseID);
            Assert.Equal(_environment, getClaimReconciliationRequest.systemEnvironmentCode);
            Assert.Equal(LanguageEnumeratedType.enUS, getClaimReconciliationRequest.languageCode);
            Assert.Equal(_fakeSettings.DealerServiceProviderSettings.SoftwareVersion,
                getClaimReconciliationRequest.versionID);

            Assert.Equal(_fakeSettings.DealerServiceProviderSettings.SoftwareName,
                getClaimReconciliationRequest.ApplicationArea.Sender.ComponentID.Value);

            Assert.Equal(ConstantSettings.VolvoWarrantyProcessingTaskIdentifier,
                getClaimReconciliationRequest.ApplicationArea.Sender.TaskID.Value);

            Assert.Equal(_fakeSettings.DealerServiceProviderSettings.Code,
                getClaimReconciliationRequest.ApplicationArea.Sender.CreatorNameCode.Value);

            Assert.Equal(_fakeSettings.DealerServiceProviderSettings.ShortCode,
                getClaimReconciliationRequest.ApplicationArea.Sender.SenderNameCode.Value);

            Assert.Equal($"{_fakeSettings.InterfaceOptions.PaCode}!",
                getClaimReconciliationRequest.ApplicationArea.Sender.DealerNumberID.Value);

            Assert.Equal(CountryEnumeratedType.CA,
                getClaimReconciliationRequest.ApplicationArea.Sender.DealerCountryCode);

            Assert.Equal(_fakeSettings.RegionSettings.LanguageCode,
                getClaimReconciliationRequest.ApplicationArea.Sender.LanguageCode);

            Assert.Equal(ConstantSettings.VolvoClaimReconciliationSenderServiceMessageId,
                getClaimReconciliationRequest.ApplicationArea.Sender.ServiceID.Value);

            Assert.True(getClaimReconciliationRequest.ApplicationArea.Sender.DealerCountryCodeSpecified);
            Assert.Equal(ConstantSettings.DestinationNameCode,
                getClaimReconciliationRequest.ApplicationArea.Destination.DestinationNameCode.Value);

            Assert.Equal(ConstantSettings.DestinationSoftwareCode,
                getClaimReconciliationRequest.ApplicationArea.Destination.DestinationSoftwareCode.Value);

            Assert.Equal(ConstantSettings.VolvoClaimReconciliationDestinationServiceMessageId,
                getClaimReconciliationRequest.ApplicationArea.Destination.ServiceMessageID.Value);

            Assert.NotNull(getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.Get);
            Assert.Single(getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.Get.Expression);
            Assert.Null(getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.Get.Expression.First()
                .Value);

            Assert.Equal(ConstantSettings.Default,
                getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First()
                    .ServiceProcessingAdvisoryHeader.DocumentIdentificationGroup.DocumentIdentification.DocumentID
                    .Value);

            Assert.Equal(claimReconciliationRequest.EndDateTime.Date,
                getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First()
                    .ServiceProcessingAdvisoryHeader.PaymentCycleEndDate);

            Assert.True(getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory
                .First()
                .ServiceProcessingAdvisoryHeader.PaymentCycleEndDateSpecified);

            Assert.Equal($"{_fakeSettings.InterfaceOptions.PaCode}!",
                getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First()
                    .ServiceProcessingAdvisoryHeader.DealerParty.PartyID.Value);

            Assert.Equal(claimReconciliationRequest.StartDateTime.Date,
                getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First()
                    .ServiceProcessingAdvisoryHeader.PaymentCycleStartDate);

            Assert.True(getClaimReconciliationRequest.GetServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory
                .First()
                .ServiceProcessingAdvisoryHeader.PaymentCycleStartDateSpecified);
        }

        [Fact]
        public void WhenSerializing_It_IncludesRootAttributes()
        {
            var claimReconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            var xml = TranslateAndSerialize(claimReconciliationRequest);

            xml.AssertContainsWithAttributes(XElement.Parse(@"
                <GetServiceProcessingAdvisory
                    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                    releaseID=""5.2.4""
                    versionID=""1.0.0""
                    systemEnvironmentCode=""Test""
                    languageCode=""en-US""
                    xmlns=""http://www.starstandard.org/STAR/5"" />"));
        }

        [Fact]
        public void WhenSerializing_It_IncludesSender()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_starNamespace + "ApplicationArea", XElement.Parse($@"
                <Sender xmlns=""{_starNamespace}"">
                    <ComponentID>Fusion</ComponentID>
                    <TaskID>WarrantyProcessing</TaskID>
                    <CreatorNameCode>Karmak</CreatorNameCode>
                    <SenderNameCode>KM</SenderNameCode>
                    <DealerNumberID>K1234!</DealerNumberID>
                    <DealerCountryCode>CA</DealerCountryCode>
                    <LanguageCode>en-US</LanguageCode>
                    <ServiceID>OWS Claim Reconciliation</ServiceID>
                </Sender>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesDestination()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_starNamespace + "ApplicationArea", XElement.Parse($@"
                 <Destination xmlns=""{_starNamespace}"">
                    <DestinationNameCode>FM</DestinationNameCode>
                    <DestinationSoftwareCode>OWS</DestinationSoftwareCode>
                    <ServiceMessageID>OWS Claim RECONCILIATION</ServiceMessageID>
                 </Destination>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesCreationDatetime()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_starNamespace + "ApplicationArea", XElement.Parse($@"
                 <CreationDateTime xmlns=""{_starNamespace}"">2019-01-01T00:00:00</CreationDateTime>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_HasEmptyExpression()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_starNamespace + "GetServiceProcessingAdvisoryDataArea", XElement.Parse($@"
                <Get xmlns=""{_starNamespace}"">
                    <Expression xmlns=""{_oagisNamespace}""/>
                </Get>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_HasDefaultIdentifier()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_owsNamespace + "ServiceProcessingAdvisoryHeaderExtended", XElement.Parse($@"
                <DocumentIdentificationGroup xmlns=""{_starNamespace}"">
                    <DocumentIdentification>
                        <DocumentID>Default</DocumentID>
                    </DocumentIdentification>
                </DocumentIdentificationGroup>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesDealerParty()
        {
            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_owsNamespace + "ServiceProcessingAdvisoryHeaderExtended", XElement.Parse($@"
                <DealerParty xmlns=""{_starNamespace}"">
                    <PartyID>K1234!</PartyID>
                </DealerParty>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesPaymentCycleStartDate()
        {
            _claimReconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_owsNamespace + "ServiceProcessingAdvisoryHeaderExtended", XElement.Parse($@"
                <PaymentCycleStartDate xmlns=""{_owsNamespace}"">2020-02-01</PaymentCycleStartDate>"));
        }

        [Fact]
        public void WhenTranslatingAndSerializing_It_IncludesPaymentCycleEndDate()
        {
            _claimReconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            var xml = TranslateAndSerialize(_claimReconciliationRequest);

            xml.AssertContainsIn(_owsNamespace + "ServiceProcessingAdvisoryHeaderExtended", XElement.Parse($@"
                <PaymentCycleEndDate xmlns=""{_starNamespace}"">2020-02-02</PaymentCycleEndDate>"));
        }

        private XDocument TranslateAndSerialize(ReconciliationFetchRequest request)
        {
            var getServiceProcessingAdvisoryType = _getClaimReconciliationTranslator.Translate(
                new GetClaimReconciliationTranslatorArguments()
                {
                    Source = request,
                    Settings = _fakeSettings
                });

            return XDocumentExtended.Serialize(getServiceProcessingAdvisoryType);
        }
    }
}
