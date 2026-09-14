using System.Xml;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.ElkContextRetrieval;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Services
{
    public class VolvoPushUpdateServiceTest
    {
        private readonly XmlDocument _SuccessfulReconDocument;
        private readonly IWarrantyElkContextRetriever _authenticationClient;
        private readonly ElkContext _validElkContext;
        private FakeShowServiceProcessingAdvisoryHandler _fakeShowServiceProcessingAdvisoryHandler;

        public VolvoPushUpdateServiceTest()
        {
            _SuccessfulReconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            _fakeShowServiceProcessingAdvisoryHandler = new FakeShowServiceProcessingAdvisoryHandler();
            _authenticationClient = Substitute.For<IWarrantyElkContextRetriever>();
            _validElkContext = new ElkContext.Builder
            {
                Identity = new ElkIdentity.Builder
                {
                    Account = Guid.NewGuid(),
                    User = Guid.NewGuid()
                }.Build(),
                SecurityProfile = new ElkIdentity.Builder
                {
                    Account = Guid.NewGuid(),
                    User = Guid.NewGuid()
                }.Build()
            }.Build();
            _authenticationClient.GetElkContextAsync(
                Arg.Any<string>()).Returns(_validElkContext);
        }

        [Fact]
        public async Task ItReturnsEmptyPutMessageResponseWhenProcessedSuccessfully()
        {
            _fakeShowServiceProcessingAdvisoryHandler = new FakeShowServiceProcessingAdvisoryHandler();
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, _fakeShowServiceProcessingAdvisoryHandler);

            var (success, response) = await service.HandleUpdate(_SuccessfulReconDocument);

            Assert.True(success);
            Assert.True(XmlHelpers.HasElement(response, "PutMessageResponse", XmlNamespaces.TransportUrl));
        }

        [Fact]
        public async Task WhenProcessingFails_It_ReturnsFailure()
        {
            var reconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, new FakeShowServiceProcessingAdvisoryHandler(false));

            var (success, response) = await service.HandleUpdate(reconDocument);

            Assert.False(success);
            Assert.True(XmlHelpers.HasElement(response, "Fault", XmlNamespaces.SoapUrl));
        }

        [Fact]
        public async Task WhenProcessingFailsToFindAPACode_It_ReturnsFailure()
        {
            var reconDocument = ResourceRetriever.GetDocument("ReconciliationMissingPACode.xml");
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, _fakeShowServiceProcessingAdvisoryHandler);

            var (success, response) = await service.HandleUpdate(reconDocument);

            Assert.False(success);
            Assert.True(XmlHelpers.HasElement(response, "Fault", XmlNamespaces.SoapUrl));
        }

        [Fact]
        public async Task WhenProcessingFailsToFindElkContextAssociatedWithAPACode_It_ReturnsFailure()
        {
            var reconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, _fakeShowServiceProcessingAdvisoryHandler);
            _authenticationClient.GetElkContextAsync(
                Arg.Any<string>()).Throws(new Exception("Context not found"));


            var (success, response) = await service.HandleUpdate(reconDocument);

            Assert.False(success);
            Assert.True(XmlHelpers.HasElement(response, "Fault", XmlNamespaces.SoapUrl));
        }

        [Fact]
        public async Task WhenProcessingSucceeds_It_SetsTheElkContext()
        {
            var reconDocument = ResourceRetriever.GetDocument("Reconciliation_Example1.xml");
            var serviceProcessingAdvisoryHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var elkContext = new List<ElkContext>();
            _authenticationClient.GetElkContextAsync(
                Arg.Any<string>()).Returns(_validElkContext);
            serviceProcessingAdvisoryHandler
                .HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>())
                .Returns(Result.Success())
                .AndDoes(callInfo => elkContext.Add(ImplicitElkContext.Current));
            var service = new VolvoPushUpdateService(
                new FakeEncryptionHandler(), Substitute.For<IExtendedLoggingClient>(),
                Substitute.For<ILogger<VolvoPushUpdateService>>(), _authenticationClient,
                serviceProcessingAdvisoryHandler);


            Assert.Empty(elkContext);
            var (success, response) = await service.HandleUpdate(reconDocument);

            Assert.True(success);
            Assert.Single(elkContext);
            Assert.Equal(_validElkContext, elkContext.First());

        }

        [Fact]
        public async Task WhenXMLIsMalformed_It_ReturnsFalse()
        {
            var reconDocument = ResourceRetriever.GetDocument("BadReconciliation_Example1.xml");
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, new FakeShowServiceProcessingAdvisoryHandler(false));

            var (success, response) = await service.HandleUpdate(reconDocument);

            Assert.False(success);
            Assert.True(XmlHelpers.HasElement(response, "Fault", XmlNamespaces.SoapUrl));

            reconDocument = ResourceRetriever.GetDocument("BadReconciliation_Example2.xml");
            (success, response) = await service.HandleUpdate(reconDocument);

            Assert.False(success);
            Assert.True(XmlHelpers.HasElement(response, "Fault", XmlNamespaces.SoapUrl));
        }



        [Fact]
        public async Task WhenProcessingAClaimReconciliationSuccess_It_CleansAdvisoryPaCode()
        {
            var serviceProcessingAdvisoryHandler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var processedRequests = new List<ShowServiceProcessingAdvisoryType>();
            serviceProcessingAdvisoryHandler
                .HandleAsync(Arg.Any<ShowServiceProcessingAdvisoryType>())
                .Returns(Result.Success())
                .AndDoes(callInfo => processedRequests.Add(callInfo.ArgAt<ShowServiceProcessingAdvisoryType>(0)));
            var service = new VolvoPushUpdateService(new FakeEncryptionHandler(),
                Substitute.For<IExtendedLoggingClient>(), Substitute.For<ILogger<VolvoPushUpdateService>>(),
                _authenticationClient, serviceProcessingAdvisoryHandler);

            var (success, response) = await service.HandleUpdate(_SuccessfulReconDocument);

            Assert.Single(processedRequests);
            Assert.Equal("02198", processedRequests.First().ApplicationArea.Destination.DealerNumberID.Value);
        }


        [Fact]
        public void WhenConstructorParametersAreNull_It_ThrowsArgumentNullException()
        {
            var logger = Substitute.For<ILogger<VolvoPushUpdateService>>();
            var extendedLogging = Substitute.For<IExtendedLoggingClient>();
            var encryptionHandler = Substitute.For<IEncryptionHandler>();
            var handler = Substitute.For<IShowServiceProcessingAdvisoryHandler>();
            var client = Substitute.For<IWarrantyElkContextRetriever>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoPushUpdateService(
                    null, extendedLogging, logger, client, handler);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoPushUpdateService(
                    encryptionHandler, null, logger, client, handler);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoPushUpdateService(
                    encryptionHandler, extendedLogging, null, client, handler);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoPushUpdateService(
                    encryptionHandler, extendedLogging, logger, null, handler);
            });
            Assert.Throws<ArgumentNullException>(() =>
            {
                new VolvoPushUpdateService(
                    encryptionHandler, extendedLogging, logger, client, null);
            });
        }
    }
}
