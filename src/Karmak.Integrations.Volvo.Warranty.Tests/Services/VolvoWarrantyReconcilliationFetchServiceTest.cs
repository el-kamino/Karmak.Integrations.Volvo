using System.Xml;
using System.Xml.Serialization;
using AutoBogus;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Services
{
    public class VolvoWarrantyReconcilliationFetchServiceTest
    {
        private readonly ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType>
            _translator;

        private readonly ISettingsProvider _settingsClient;
        private readonly IVolvoClient _volvoClient;
        private readonly ISoapRequestFactory _soapRequestFactory;
        private readonly ReconciliationFetchRequest _claimsReconciliationsFetchRequest;
        private readonly XmlDocument _SuccessfulReconDocument;
        private readonly IExtendedLoggingClient _extendedLoggingClient = Substitute.For<IExtendedLoggingClient>();
        private readonly FakeShowServiceProcessingAdvisoryHandler _fakeShowServiceProcessingAdvisoryHandler;
        private readonly VolvoSettings _volvoSettings;

        public VolvoWarrantyReconcilliationFetchServiceTest()
        {
            _SuccessfulReconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            _claimsReconciliationsFetchRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };
            _fakeShowServiceProcessingAdvisoryHandler = new FakeShowServiceProcessingAdvisoryHandler();
            _translator =
                Substitute.For<ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType>>();

            _settingsClient = Substitute.For<ISettingsProvider>();
            _volvoClient = Substitute.For<IVolvoClient>();
            _soapRequestFactory = Substitute.For<ISoapRequestFactory>();

            _translator
                .Translate(Arg.Any<GetClaimReconciliationTranslatorArguments>());

            _volvoSettings = AutoFaker.Generate<VolvoSettings>();
            _volvoSettings.InterfaceOptions.WarrantyEnabled = true;

            _settingsClient.GetSettingsAsync().Returns(_volvoSettings);

            _soapRequestFactory.CreateProcessRequest(Arg.Any<SoapMessageAddress>(), Arg.Any<GetServiceProcessingAdvisoryType>())
                .Returns(new SoapEnvelope());

            _volvoClient.OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Success(_SuccessfulReconDocument));
        }

        [Fact]
        public void ItCanDeserializeAReconciliationResponse()
        {
            var reconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            var processingAdvisory =
                reconDocument.GetElementsByTagName("ShowServiceProcessingAdvisory", XmlNamespaces.StarUrl)[0];

            ShowServiceProcessingAdvisoryType deserializedProcessingAdvisory;

            var serializer = new XmlSerializer(typeof(ShowServiceProcessingAdvisoryType));

            using (XmlReader reader = new XmlNodeReader(processingAdvisory))
            {
                deserializedProcessingAdvisory = (ShowServiceProcessingAdvisoryType)serializer.Deserialize(reader);
            }

            Assert.NotNull(deserializedProcessingAdvisory.ShowServiceProcessingAdvisoryDataArea
                .ServiceProcessingAdvisory);
        }

        [Fact]
        public async Task ItCallsGetClaimReconciliationTranslator()
        {
            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                _fakeShowServiceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            await service.TriggerReconciliationFetch(_claimsReconciliationsFetchRequest);

            _translator.Received(1).Translate(
                Arg.Is<GetClaimReconciliationTranslatorArguments>(x => x.Source == _claimsReconciliationsFetchRequest)
            );
        }

        [Fact]
        public async Task WhenProcessingAClaimReconciliationWithoutARecognizedBody_It_ThrowsAnException()
        {
            var serviceProcessingAdvisoryHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var request = new List<ShowServiceProcessingAdvisoryType>();

            serviceProcessingAdvisoryHandler
                .HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>())
                .Returns(Result.Success())
                .AndDoes(callInfo => request.Add(callInfo.ArgAt<ShowServiceProcessingAdvisoryType>(0)));

            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                serviceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            _volvoClient.OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Success(new XmlDocument()));

            var exception = await Assert.ThrowsAsync<ReconcilliationFetchException>(() => service.TriggerReconciliationFetch(_claimsReconciliationsFetchRequest));
            Assert.Contains("TryExtractShowServiceProcessingAdvisoryType did not find a valid message in the body.", exception.Message);
        }

        [Fact]
        public async Task WhenAClaimReconciliationFailsToProcess_It_ThrowsAnException()
        {
            var serviceProcessingAdvisoryHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var request = new List<ShowServiceProcessingAdvisoryType>();
            var error = "Error";
            var errorReason = "Error Reason";
            var errorDetails = "Error Details";
            serviceProcessingAdvisoryHandler
                .HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>())
                .Returns(Result.Fault(error, errorReason, errorDetails))
                .AndDoes(callInfo => request.Add(callInfo.ArgAt<ShowServiceProcessingAdvisoryType>(0)));
            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                serviceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);
            _volvoClient.OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Success(_SuccessfulReconDocument));

            var exception = await Assert.ThrowsAsync<ReconcilliationFetchException>(() => service.TriggerReconciliationFetch(_claimsReconciliationsFetchRequest));
            Assert.Contains(error, exception.Message);
            Assert.Contains(errorReason, exception.Message);
            Assert.Contains(errorDetails, exception.Message);
        }

        [Fact]
        public async Task WhenProcessingAClaimReconciliationSuccess_It_CallsServiceProcessingAdvisoryHandler()
        {
            var serviceProcessingAdvisoryHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var request = new List<ShowServiceProcessingAdvisoryType>();
            serviceProcessingAdvisoryHandler
                .HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>())
                .Returns(Result.Success())
                .AndDoes(callInfo => request.Add(callInfo.ArgAt<ShowServiceProcessingAdvisoryType>(0)));
            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                serviceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            await service.TriggerReconciliationFetch(_claimsReconciliationsFetchRequest);

            await serviceProcessingAdvisoryHandler.Received(1).HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>());
        }


        [Fact]
        public async Task WhenMakingRequest_It_FetchesSettings()
        {
            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                _fakeShowServiceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            await service.TriggerReconciliationFetch(_claimsReconciliationsFetchRequest);

            await _settingsClient.Received(1).GetSettingsAsync();
        }

        [Fact]
        public async Task ItCallsVolvoClient()
        {
            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, _settingsClient,
                _fakeShowServiceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            var capturedAddresses = new List<SoapMessageAddress>();
            _soapRequestFactory.CreateProcessRequest(Arg.Any<SoapMessageAddress>(), Arg.Any<GetServiceProcessingAdvisoryType>())
                .Returns(new SoapEnvelope())
                .AndDoes(callInfo => capturedAddresses.Add(callInfo.ArgAt<SoapMessageAddress>(0)));

            _volvoClient.OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Success(_SuccessfulReconDocument));

            var reconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            await service.TriggerReconciliationFetch(reconciliationRequest);

            await _volvoClient.Received(1).OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>());
            Assert.Single(capturedAddresses);

            var address = capturedAddresses.First();
            Assert.Equal(ConstantSettings.ClaimReconciliationRequest, address.To);
            Assert.Equal(ConstantSettings.StarProcessMessageAction, address.Action);
            Assert.Equal(ConstantSettings.VolvoGetServiceProcessingAdvisory, address.TargetService);
            Assert.Equal(ConstantSettings.VolvoOneWarrantySystemVersion, address.TargetServiceVersion);
            Assert.Equal(_volvoSettings.RegionSettings.CountryCode + _volvoSettings.InterfaceOptions.PaCode,
                address.SiteCode);
        }

        [Fact]
        public void WhenConstructorParametersAreNull_It_ThrowsArgumentNullException()
        {
            var logger = Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>();
            var translator = Substitute.For<ITranslatable<GetClaimReconciliationTranslatorArguments, GetServiceProcessingAdvisoryType>>();
            var volvoClient = Substitute.For<IVolvoClient>();
            var volvoSettingClient = Substitute.For<ISettingsProvider>();
            var handler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var soapRequestFactory = Substitute.For<ISoapRequestFactory>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    null, translator, volvoClient, volvoSettingClient, handler, _extendedLoggingClient, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, null, volvoClient, volvoSettingClient, handler, _extendedLoggingClient, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, translator, null, volvoSettingClient, handler, _extendedLoggingClient, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, translator, volvoClient, null, handler, _extendedLoggingClient, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, translator, volvoClient, volvoSettingClient, null, _extendedLoggingClient, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, translator, volvoClient, volvoSettingClient, handler, null, soapRequestFactory);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoWarrantyReconcilliationFetchService(
                    logger, translator, volvoClient, volvoSettingClient, handler, _extendedLoggingClient, null);
            });
        }

        [Theory]
        [InlineData(true, true, 1)]
        [InlineData(true, false, 1)]
        [InlineData(null, null, 1)]
        [InlineData(false, true, 0)]
        [InlineData(false, false, 0)]
        public async Task WhenWarrantyIsNotDisabled_ReconciliationUpdates_AreRequested(bool? warrantyFlag, bool? reactFlag, int expectedCallCount)
        {
            var settings = AutoFaker.Generate<VolvoSettings>();
            settings.InterfaceOptions.WarrantyEnabled = warrantyFlag;
            settings.InterfaceOptions.ReactEnabled = reactFlag;

            var settingsClient = Substitute.For<ISettingsProvider>();
            settingsClient.GetSettingsAsync().Returns(settings);

            var service = new VolvoWarrantyReconcilliationFetchService(Substitute.For<ILogger<VolvoWarrantyReconcilliationFetchService>>(),
                _translator, _volvoClient, settingsClient,
                _fakeShowServiceProcessingAdvisoryHandler, _extendedLoggingClient, _soapRequestFactory);

            var reconciliationRequest = new ReconciliationFetchRequest
            {
                StartDateTime = new DateTime(2020, 2, 1),
                EndDateTime = new DateTime(2020, 2, 2)
            };

            await service.TriggerReconciliationFetch(reconciliationRequest);

            await _volvoClient.Received(expectedCallCount).OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>());
        }
    }
}
