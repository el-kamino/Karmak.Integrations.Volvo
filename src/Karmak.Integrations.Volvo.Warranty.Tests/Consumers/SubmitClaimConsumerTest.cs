using System.Net;
using System.Xml;
using AutoBogus;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Consumers;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Translators;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Consumers
{
    public class SubmitClaimConsumerTest
    {
        private const string PushApiUri = "http://example.karmak.io";

        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType> _translator;
        private readonly ISettingsProvider _settingsClient;
        private readonly MessageConsumeContext<SubmitClaimPayload> _fakeMessageConsumeContext;
        private readonly SubmitClaimPayload _submitClaimCommand;
        private readonly IVolvoClient _volvoClient;
        private readonly IClaimsService _claimsService;
        private readonly SubmitClaimConsumer _consumer;
        private readonly VolvoSettings _defaultSettings;

        public SubmitClaimConsumerTest()
        {
            var logger = Substitute.For<ILogger<SubmitClaimConsumer>>();
            _extendedLoggingClient = Substitute.For<IExtendedLoggingClient>();
            _translator = Substitute.For<ITranslatable<SubmitClaimTranslatorArguments, ProcessRepairOrderType>>();
            _settingsClient = Substitute.For<ISettingsProvider>();
            _volvoClient = Substitute.For<IVolvoClient>();
            _claimsService = Substitute.For<IClaimsService>();

            _defaultSettings = AutoFaker.Generate<VolvoSettings>();
            _defaultSettings.InterfaceOptions.WarrantyEnabled = true;

            _translator
                .Translate(Arg.Any<SubmitClaimTranslatorArguments>())
                .Returns(new ProcessRepairOrderType());
            _settingsClient
                .GetSettingsAsync()
                .Returns(_defaultSettings);
            _volvoClient
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Success(new XmlDocument()));
            _claimsService
                .UpdateStatus(
                    Arg.Is<string>(x => x != null),
                    Arg.Is<ClaimStatus>(x => x != null),
                    Arg.Any<FusionIdentity>())
                .Returns(FakeClaim.Generate());

            var options = Options.Create(new VolvoTransportConfigurationOptions
            {
                CallbackUri = PushApiUri
            });

            _consumer = new SubmitClaimConsumer(
                logger,
                _extendedLoggingClient,
                _translator,
                _settingsClient,
                _volvoClient,
                _claimsService,
                options);

            var fakeHeaders = Substitute.For<Headers>();
            var fakeConsumeContext = Substitute.For<ConsumeContext>();
            fakeConsumeContext
                .Headers
                .Returns(fakeHeaders);
            _submitClaimCommand = new SubmitClaimPayload(FakeClaim.Generate(5));
            _fakeMessageConsumeContext = new MessageConsumeContext<SubmitClaimPayload>(fakeConsumeContext, _submitClaimCommand);
        }

        [Fact]
        public async Task WhenConsumingAMessage_It_StoresMessageInExtendedLogging()
        {
            await _consumer.Consume(_fakeMessageConsumeContext);

            await _extendedLoggingClient.Received()
                .Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task WhenTranslatingAMessageFails_It_Throws()
        {
            var exception = new Exception("Something went wrong ...");
            _translator
                .Translate(Arg.Any<SubmitClaimTranslatorArguments>())
                .Throws(exception);

            var thrownException = await Assert.ThrowsAsync<Exception>(() => _consumer.Consume(_fakeMessageConsumeContext));

            Assert.Equal(exception, thrownException);
        }


        [Fact]
        public async Task WhenConsumingAMessage_It_FetchesSettings()
        {
            await _consumer.Consume(_fakeMessageConsumeContext);

            await _settingsClient.Received(1).GetSettingsAsync();
        }

        [Fact]
        public async Task WhenConsumingAMessage_It_SendsARequestToVolvo()
        {
            await _consumer.Consume(_fakeMessageConsumeContext);

            await _volvoClient.Received(1)
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>());
        }

        [Fact]
        public async Task WhenSendingARequestToVolvoSucceeds_It_UpdatesAllClaimStatus()
        {
            await _consumer.Consume(_fakeMessageConsumeContext);

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received()
                    .UpdateStatus(claim.Id, ClaimStatus.Submitted, Arg.Any<FusionIdentity>());
            }
        }

        [Fact]
        public async Task WhenUpdatingClaimStatus_It_UsesASystemUser()
        {
            await _consumer.Consume(_fakeMessageConsumeContext);

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received()
                    .UpdateStatus(claim.Id, ClaimStatus.Submitted,
                        Arg.Is<FusionIdentity>(fusionIdentity => fusionIdentity.Username == FusionIdentity.SystemUser.Username));
            }
        }

        [Fact]
        public async Task WhenSendingARequestToVolvoFails_It_ThrowsAnException()
        {
            _volvoClient
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Failure(HttpStatusCode.InternalServerError, new XmlDocument()));

            await Assert.ThrowsAsync<OWSClaimSubmissionException>(() => _consumer.Consume(_fakeMessageConsumeContext));

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received()
                    .UpdateStatus(claim.Id, ClaimStatus.Failed, Arg.Any<FusionIdentity>());
            }
        }

        [Fact]
        public async Task WhenSendingARequestToVolvoReturnsInvalidSignature_It_ThrowsAnException()
        {
            _volvoClient
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.InvalidSignature(new XmlDocument()));

            await Assert.ThrowsAsync<OWSClaimSubmissionException>(() => _consumer.Consume(_fakeMessageConsumeContext));

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received().UpdateStatus(claim.Id, ClaimStatus.Failed, Arg.Any<FusionIdentity>());
            }
        }

        [Fact]
        public async Task WhenSendingARequestToVolvoReturnsErrorWithResponse_It_ThrowsAnException()
        {
            _volvoClient
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Error
                {
                    Response = new string("")
                });

            await Assert.ThrowsAsync<OWSClaimSubmissionException>(() => _consumer.Consume(_fakeMessageConsumeContext));

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received().UpdateStatus(claim.Id, ClaimStatus.Failed, Arg.Any<FusionIdentity>());
            }
        }

        [Fact]
        public async Task WhenSendingARequestToVolvoReturnsErrorWithException_It_ThrowsAnException()
        {
            _volvoClient
                .OAuthSendSoapAsync(Arg.Is<VolvoSoapRequest>(x => x != null), Arg.Any<IDictionary<string, string>>())
                .Returns(new SoapResult.Error
                {
                    Exception = new Exception("Failure!")
                });

            await Assert.ThrowsAsync<OWSClaimSubmissionException>(() => _consumer.Consume(_fakeMessageConsumeContext));

            foreach (var claim in _submitClaimCommand.Claims)
            {
                await _claimsService.Received().UpdateStatus(claim.Id, ClaimStatus.Failed, Arg.Any<FusionIdentity>());
            }
        }
    }
}
