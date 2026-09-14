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
    public sealed class ProcessClaimReconciliationHandlerTests
    {
        private readonly IInboundMessageSender _sender;
        private readonly IReconciliationToUpdateSnapshotMapper _mapper;
        private readonly IPushValidationHandler _validationHandler;
        private readonly ILogger<ProcessClaimReconciliationHandler> _logger;
        private readonly IExtendedLoggingClient _extendedLoggingClient;
        private readonly IUpdateSnapshotCorrelationHandler _correlationHandler;
        private readonly IReconciliationToUpdateSnapshotMapper _realMapper;

        public ProcessClaimReconciliationHandlerTests()
        {
            _sender = Substitute.For<IInboundMessageSender>();
            _mapper = Substitute.For<IReconciliationToUpdateSnapshotMapper>();
            _validationHandler = Substitute.For<IPushValidationHandler>();
            _logger = Substitute.For<ILogger<ProcessClaimReconciliationHandler>>();
            _extendedLoggingClient = Substitute.For<IExtendedLoggingClient>();
            _correlationHandler = Substitute.For<IUpdateSnapshotCorrelationHandler>();
            _realMapper = new ReconciliationToUpdateSnapshotMapper();

            _mapper.Map(Arg.Any<RepairOrderReconciliationType>(),
                Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<Claim>(), Arg.Any<DateTime>())
                .Returns(AutoFaker.Generate<UpdateSnapshot>());

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => CreateValidResult(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0)));


            _correlationHandler.Correlate(Arg.Any<string>())
                .Returns(AutoFaker.Generate<Claim>(3));
        }

        [Fact]
        public void WhenCtorParametersAreNull_It_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(null, _mapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(_sender, null, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(_sender, _mapper, null, _logger, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(_sender, _mapper, _validationHandler, null, _extendedLoggingClient, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(_sender, _mapper, _validationHandler, _logger, null, _correlationHandler); });
            Assert.Throws<ArgumentNullException>(() => { new ProcessClaimReconciliationHandler(_sender, _mapper, _validationHandler, _logger, _extendedLoggingClient, null); });
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CallsTheInboundMessageSender()
        {
            var h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(FakeShowServiceProcessingAdvisoryType.Generate()));

            Assert.True(result.Succeeded);
            await _sender.Received(1).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenMappingUpdateSnapshotThrowsAnException_It_ContinuesProcessingUpdates()
        {
            var fakeRepairOrderReconciliations = FakeRepairOrderReconciliationType.Generate(2).ToArray();
            var fake = FakeShowServiceProcessingAdvisoryType.Generate();
            fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation = fakeRepairOrderReconciliations;

            //Throw an exception on the first call, return valid value on subsequent calls
            _mapper.Map(Arg.Any<RepairOrderReconciliationType>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<Claim>(), Arg.Any<DateTime>())
                .Returns(
                    callInfo => throw new Exception("Big bad exception."),
                    callInfo => AutoFaker.Generate<UpdateSnapshot>());

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo =>
                {
                    var x = callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0);
                    return x.Select(y => y.UpdateSnapshot != null ?
                    new ValidatedUpdateSnapshotMessage
                    {
                        Errors = null,
                        UpdateSnapshotMessage = y
                    } : AutoFaker.Generate<ValidatedUpdateSnapshotMessage>());
                });

            var h = new ProcessClaimReconciliationHandler(_sender, _mapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(fake));

            //A success should only be returned if all updates process successfully
            Assert.False(result.Succeeded);
            await _sender.Received(1).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_TransformsRepairOrderReconciliationsToUpdateSnapshotMessages()
        {
            var fakeRepairOrderReconciliations = FakeRepairOrderReconciliationType.Generate(3).ToArray();
            var fake = FakeShowServiceProcessingAdvisoryType.Generate();
            fake.ApplicationArea.Destination.DealerNumberID.Value = "12345!";
            fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation = fakeRepairOrderReconciliations;

            List<UpdateSnapshotMessage> sentMessages = new List<UpdateSnapshotMessage> { };
            _sender.PublishAsync(Arg.Any<UpdateSnapshotMessage>())
                .Returns(Task.CompletedTask)
                .AndDoes(callInfo => sentMessages.Add(callInfo.ArgAt<UpdateSnapshotMessage>(0)));

            var h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(fake));

            var expectedProcessDate = fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().ProcessDate;

            Assert.True(result.Succeeded);
            Assert.Equal(3, sentMessages.Count);
            for (int i = 0; i < sentMessages.Count; i++)
            {
                var expectedApprovedAmount = fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation[i].JobReconciliation.First().ApprovedAmount.Value;
                var actualApprovedAmount = sentMessages[i].UpdateSnapshot.ApprovedAmount.Value;

                Assert.Equal(expectedProcessDate, sentMessages[i].UpdateSnapshot.ProcessDate);
                Assert.Equal(expectedApprovedAmount, actualApprovedAmount);
                Assert.Equal("12345", sentMessages[i].UpdateSnapshot.DealerCode);
                Assert.NotNull(sentMessages[i].TransactionId);
                Assert.Equal(DateTime.UtcNow, sentMessages[i].UpdateSnapshot.CreatedDateTime.UtcDateTime, TimeSpan.FromSeconds(1));
            }
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CallsTheInboundMessageSenderForValidUpdates()
        {
            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            await _sender.Received(3).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenASingleReconciliationsContainMultipleJobs_It_CallsTheInboundMessageSenderForEachJobInTheReconciliation()
        {
            var jobReconciliations = AutoFaker.Generate<JobReconciliationExtended>(2);

            var reconciliation = CreateFakeResponse(1);
            reconciliation.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory.First().ServiceProcessingAdvisoryHeader.DispositionPayments.First().RepairOrderReconciliation.First().JobReconciliation = jobReconciliations.ToArray();
            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(reconciliation));

            Assert.True(result.Succeeded);
            await _sender.Received(2).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_DoesNotCallTheInboundMessageSenderForInvalidUpdates()
        {
            var h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => AutoFaker.Generate<ValidatedUpdateSnapshotMessage>(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0).Count()).ToList());

            var result = await h.HandleAsync(CreateFakeResponse());

            Assert.False(result.Succeeded);
            await _sender.Received(0).PublishAsync<UpdateSnapshotMessage>(Arg.Any<UpdateSnapshotMessage>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalled_It_CorrelatesEachUpdate()
        {
            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
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

            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

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

            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

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

            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

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

            ProcessClaimReconciliationHandler h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.True(result.Succeeded);
            Assert.DoesNotContain(sentMessages, x => x.ClaimId != null);
            await _correlationHandler.Received(3).Correlate(Arg.Any<string>());
        }

        [Fact]
        public async Task WhenHandleAsyncIsCalledAndInvalidUpdatesAreFound_It_ReturnsAnInvalidUpdateFailure()
        {
            var h = new ProcessClaimReconciliationHandler(_sender, _realMapper, _validationHandler, _logger, _extendedLoggingClient, _correlationHandler);

            _validationHandler.Validate(Arg.Any<IEnumerable<UpdateSnapshotMessage>>())
                .Returns(callInfo => AutoFaker.Generate<ValidatedUpdateSnapshotMessage>(callInfo.ArgAt<IEnumerable<UpdateSnapshotMessage>>(0).Count()));

            var result = await ImplicitElkContext.WithCurrentAsync(AutoFaker.Generate<ElkContext>(), () => h.HandleAsync(CreateFakeResponse()));

            Assert.False(result.Succeeded);
            Assert.Equal("Message contained invalid reconciliation updates.", result.FaultCode);
        }

        private ShowServiceProcessingAdvisoryType CreateFakeResponse(int serviceProcessingAdvisoryCount = 3)
        {
            var fakeServiceProcessingAdvisories = FakeServiceProcessingAdvisoryType.Generate(serviceProcessingAdvisoryCount).ToArray();
            var fake = FakeShowServiceProcessingAdvisoryType.Generate();
            fake.ShowServiceProcessingAdvisoryDataArea.ServiceProcessingAdvisory = fakeServiceProcessingAdvisories;

            return fake;
        }

        private IEnumerable<ValidatedUpdateSnapshotMessage> CreateValidResult(IEnumerable<UpdateSnapshotMessage> updates)
        {
            return updates.Select(x => new ValidatedUpdateSnapshotMessage
            {
                Errors = null,
                UpdateSnapshotMessage = x
            });
        }
    }
}
