using AutoBogus;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes.OWS;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Handlers
{
    public sealed class ProcessClaimStatusHandlerTests
    {
        private readonly IInboundMessageSender _sender = Substitute.For<IInboundMessageSender>();
        private readonly IStatusToUpdateSnapshotMapper _mapper = Substitute.For<IStatusToUpdateSnapshotMapper>();
        private readonly IPushValidationHandler _validationHandler = Substitute.For<IPushValidationHandler>();
        private readonly ILogger<ProcessClaimStatusHandler> _logger = Substitute.For<ILogger<ProcessClaimStatusHandler>>();
        private readonly IExtendedLoggingClient _extendedLoggingClient = Substitute.For<IExtendedLoggingClient>();
        private readonly IUpdateSnapshotCorrelationHandler _correlationHandler = Substitute.For<IUpdateSnapshotCorrelationHandler>();
        private readonly IStatusToUpdateSnapshotMapper _realMapper = new StatusToUpdateSnapshotMapper();

        public ProcessClaimStatusHandlerTests()
        {
            _mapper.Map(Arg.Any<RepairOrderReconciliationType>(), Arg.Any<DateTime>(), Arg.Any<string>())
                .Returns(AutoFaker.Generate<UpdateSnapshot>());

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => CreateValidResult(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0)));

            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns(AutoFaker.Generate<Claim>(3));
        }

        [Fact]
        public void WhenCtorParametersAreNull_It_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(null, _mapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, null, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _mapper, null, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _mapper, _validationHandler, null, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _mapper, _validationHandler, _logger, null, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _mapper, _validationHandler, _logger, _extendedLoggingClient, null); });
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CallsTheInboundMessageSender()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(FakeShowServiceProcessingAdvisoryType.Generate()));

            Assert.True(result.Succeeded);
            await _sender.Received(1).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_TransformsRepairOrderReconciliationsToUpdateSnapshotMessages()
        {
            var fakeRepairOrderReconciliations = FakeRepairOrderReconciliationType.Generate(3).ToArray();
            var fake = FakeShowServiceProcessingAdvisoryType.Generate();
            fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation = fakeRepairOrderReconciliations;

            var sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(fake));

            var expectedProcessDate = fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DocumentDateTime;

            Assert.True(result.Succeeded);
            Assert.Equal(3, sentMessages.Count);
            for (int i = 0; i < sentMessages.Count; i++)
            {
                var expectedApprovedAmount = fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation[i].JobReconciliation.First().ApprovedAmount.Value;
                var actualApprovedAmount = sentMessages[i].UpdateSnapshot.ApprovedAmount.Value;

                Assert.Equal(expectedProcessDate, sentMessages[i].UpdateSnapshot.ProcessDate);
                Assert.Equal(expectedApprovedAmount, actualApprovedAmount);
                Assert.NotNull(sentMessages[i].TransactionId);
            }
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CallsTheInboundMessageSenderForEachValidUpdate()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo =>
                {
                    var updates = CreateValidResult(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0));
                    updates.First().Errors = new[] { "error" };
                    return updates;
                });

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.False(result.Succeeded);
            await _sender.Received(2).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsync_It_DoesNotCallTheInboundMessageSenderForInvalidUpdates()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => AutoFaker.Generate<ValidatedUpdateSnapshotMessage>(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0).Count()).ToList());

            var result = await h.HandleAsync(CreateFakeResponse());

            Assert.False(result.Succeeded);
            await _sender.Received(0).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CorrelatesEachUpdateWithAClaim()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);
            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalledAndFails_It_ReturnsAFailure()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            Result response = await h.HandleAsync(null);

            Assert.False(response.Succeeded);
            Assert.Equal("Error occurred, status update processing failed.", response.FaultCode);
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalledAndInvalidUpdatesAreFound_It_ReturnsAnInvalidUpdateFailure()
        {
            var h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => AutoFaker.Generate<ValidatedUpdateSnapshotMessage>(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0).Count()).ToList());

            Result response = await h.HandleAsync(CreateFakeResponse());

            Assert.False(response.Succeeded);
            Assert.Equal("Message contained invalid status updates.", response.FaultCode);
        }

        [Fact]
        public async Task WhenCorrelationHandlerReturnsNull_It_CreatesUpdatesWithoutClaim()
        {
            List<UpdateSnapshotMessage> sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns((IEnumerable<Claim>)null);

            ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            Assert.DoesNotContain(sentMessages, x => x.ClaimId != null);
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenCorrelationHandlerReturnsOneClaim_It_CreatesUpdatesUsingTheClaim()
        {
            List<UpdateSnapshotMessage> sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            var claims = AutoFaker.Generate<Claim>(1);
            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns(claims);

            ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            Assert.True(sentMessages.All(x => x.ClaimId == claims.First().Id));
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenCorrelationHandlerReturnsMultipleClaims_It_CreatesUpdatesByMatchingJobIdentifiers()
        {
            List<UpdateSnapshotMessage> sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            var fakeResponse = CreateFakeResponse();
            var jobNumbers = fakeResponse.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.SelectMany(advisory =>
            {
                return advisory.ServiceProcessingAdvisoryHeader.DispositionPayments.SelectMany(payment =>
                {
                    return payment.RepairOrderReconciliation.SelectMany(reconciliation =>
                    {
                        return reconciliation.JobReconciliation.Select(job =>
                        {
                            return job.JobNumberString;
                        });
                    });
                });
            }).ToList();

            var claims = AutoFaker.Generate<Claim>(3);
            var claimIds = new List<string>();
            for (int i = 0; i < claims.Count; i++)
            {
                claims[i].RepairOrder.Jobs.First().Identifier = jobNumbers[i];
                claimIds.Add(claims[i].Id);
            }
            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns(claims);

            ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(fakeResponse));

            Assert.True(result.Succeeded);
            Assert.NotEmpty(sentMessages);
            foreach (var message in sentMessages)
            {
                Assert.Contains(message.ClaimId, claimIds);
                claimIds.Remove(message.ClaimId);
            }
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenCorrelationHandlerReturnsMultipleClaims_It_CreatesUpdatesWithoutClaimIfNoMatchingJobIdentifiers()
        {
            List<UpdateSnapshotMessage> sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            var claims = AutoFaker.Generate<Claim>(3);
            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns(claims);

            ProcessClaimStatusHandler h = new ProcessClaimStatusHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            Assert.DoesNotContain(sentMessages, x => x.ClaimId != null);
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        private ShowServiceProcessingAdvisoryType CreateFakeResponse()
        {
            var fakeServiceProcessingAdvisories = FakeServiceProcessingAdvisoryType.Generate(3).ToArray();
            var fake = FakeShowServiceProcessingAdvisoryType.Generate();
            fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory = fakeServiceProcessingAdvisories;

            return fake;
        }

        private IList<ValidatedUpdateSnapshotMessage> CreateValidResult(IEnumerable<UpdateSnapshotMessage> updates)
        {
            var x = new List<ValidatedUpdateSnapshotMessage>();
            foreach (var update in updates)
            {
                x.Add(new ValidatedUpdateSnapshotMessage
                {
                    Errors = null,
                    UpdateSnapshotMessage = update
                });
            }
            return x;
        }
    }
}
