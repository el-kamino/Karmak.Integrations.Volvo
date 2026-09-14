using Karmak.Integrations.Volvo.Warranty.Consumers;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Consumers
{
    public class SubmitClaimFaultConsumerTest
    {
        private readonly IClaimsService _claimsService;
        private readonly SubmitClaimFaultConsumer _consumer;
        private readonly SubmitClaimPayload _payload;
        private readonly ConsumeContext<Fault<SubmitClaimPayload>> _context;

        public SubmitClaimFaultConsumerTest()
        {
            _claimsService = Substitute.For<IClaimsService>();
            _consumer = new SubmitClaimFaultConsumer(Substitute.For<ILogger<SubmitClaimFaultConsumer>>(), _claimsService);
            _payload = new SubmitClaimPayload(FakeClaim.Generate(5));

            var faultMessage = Substitute.For<Fault<SubmitClaimPayload>>();
            faultMessage.Message.Returns(_payload);
            faultMessage.Exceptions.Returns(Array.Empty<ExceptionInfo>());

            _context = Substitute.For<ConsumeContext<Fault<SubmitClaimPayload>>>();
            _context.Message.Returns(faultMessage);
            _context.Headers.Returns(Substitute.For<Headers>());
        }

        [Fact]
        public async Task WhenConsumingFaultedSubmitClaimCommand_It_UpdatesAllClaimStatusToFailed()
        {
            await _consumer.Consume(_context);

            foreach (var claim in _payload.Claims)
            {
                await _claimsService.Received(1)
                    .UpdateStatus(claim.Id, ClaimStatus.Failed, FusionIdentity.Volvo);
            }
        }
    }
}
