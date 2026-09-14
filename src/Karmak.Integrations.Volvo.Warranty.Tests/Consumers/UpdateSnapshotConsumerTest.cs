using AutoBogus;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;
using Karmak.Integrations.Volvo.Warranty.Consumers;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using MassTransit;
using MassTransit.Context;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Consumers
{
    public class UpdateSnapshotConsumerTest
    {
        private readonly ElkContext _fakeElkContext;
        private readonly ILogger<UpdateSnapshotConsumer> _logger;
        private readonly ConsumeContext _consumeContext;
        private readonly IClaimsService _claimsService;
        private readonly IFusionClient _fusionClient;
        private readonly List<UpdateSnapshot> _appliedUpdateSnapshots;
        private readonly Claim _claim;

        public UpdateSnapshotConsumerTest()
        {
            var fakeHeaders = Substitute.For<Headers>();
            _fakeElkContext = AutoFaker.Generate<ElkContext>();
            _logger = Substitute.For<ILogger<UpdateSnapshotConsumer>>();
            _claimsService = Substitute.For<IClaimsService>();
            _fusionClient = Substitute.For<IFusionClient>();
            _consumeContext = Substitute.For<ConsumeContext>();
            _claim = AutoFaker.Generate<Claim>();
            _appliedUpdateSnapshots = new List<UpdateSnapshot>();

            _consumeContext.Headers
                .Returns(fakeHeaders);

            _claimsService.AddToUpdateSnapshotHistory(Arg.Any<string>(), Arg.Any<UpdateSnapshot>(), Arg.Any<FusionIdentity>())
                .Returns(_claim)
                .AndDoes(callInfo => _appliedUpdateSnapshots.Add(callInfo.ArgAt<UpdateSnapshot>(1)));

            _claimsService.Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Any<FusionIdentity>());

            _fusionClient.ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>());

        }

        private void SetupAddToUpdateSnapshotHistory(UpdateSnapshotMessage update)
        {
            _claimsService.AddToUpdateSnapshotHistory(Arg.Any<string>(), Arg.Any<UpdateSnapshot>(), Arg.Any<FusionIdentity>())
                .Returns(CreateClaimWithUpdate(update));
        }

        [Fact]
        public async Task WhenConsumingReconciliationMessage_It_AddsUpdateToSnapshotHistory()
        {
            var reconciliationUpdate = CreateReconciliationUpdate(_claim);
            var reconConsumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, reconciliationUpdate);

            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(reconConsumeMessageContext));

            Assert.Single(_appliedUpdateSnapshots);
            Assert.Contains(reconciliationUpdate.UpdateSnapshot, _appliedUpdateSnapshots);
            await _claimsService.Received(1).AddToUpdateSnapshotHistory(Arg.Any<string>(), Arg.Any<UpdateSnapshot>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoStatus.Username));
        }

        [Fact]
        public async Task WhenConsumingStatusMessage_It_AddsUpdateToSnapshotHistory()
        {
            var statusUpdate = CreateStatusUpdate(_claim);
            var statusConsumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, statusUpdate);

            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(statusConsumeMessageContext));

            Assert.Single(_appliedUpdateSnapshots);
            Assert.Contains(statusUpdate.UpdateSnapshot, _appliedUpdateSnapshots);
            await _claimsService.Received(1).AddToUpdateSnapshotHistory(Arg.Any<string>(), Arg.Any<UpdateSnapshot>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoStatus.Username));
        }

        [Fact]
        public async Task WhenConsumingUnprocessedReconciliationUpdate_It_CallsTheFusionClientWithPaymentInformation()
        {
            var reconciliationUpdate = CreateReconciliationUpdate(_claim);
            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, reconciliationUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            SetupAddToUpdateSnapshotHistory(reconciliationUpdate);

            WarrantyPaymentInformation sentPaymentInfo = null;
            _fusionClient.ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>())
                .ReturnsForAnyArgs(Task.CompletedTask)
                .AndDoes(callInfo => sentPaymentInfo = callInfo.ArgAt<WarrantyPaymentInformation>(0));

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _fusionClient.Received(1)
                .ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>());
            Assert.NotNull(sentPaymentInfo);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.Id, sentPaymentInfo.Id);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.RepairOrderNumber, sentPaymentInfo.ClaimNumber);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.DealerCode, sentPaymentInfo.DealerCode);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.ProcessDate, sentPaymentInfo.ProcessDate);
            Assert.Equal(default(int), sentPaymentInfo.RepairOrderID);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.RepairOrderNumber, sentPaymentInfo.RepairOrderNumber);
            Assert.Equal(reconciliationUpdate.UpdateSnapshot.ApprovedAmount.Value, sentPaymentInfo.ToBePaidAmount);
        }

        [Fact]
        public async Task WhenConsumingUnprocessedReconciliationUpdate_It_SetsProcessedByFusionDateTime()
        {
            var reconciliationUpdate = CreateReconciliationUpdate(_claim);
            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, reconciliationUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            SetupAddToUpdateSnapshotHistory(reconciliationUpdate);

            DateTimeOffset? processedDateTime = null;
            _claimsService.Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoRecon.Username))
                .ReturnsForAnyArgs(callInfo => callInfo.ArgAt<Claim>(0))
                .AndDoes(callInfo =>
                    processedDateTime = callInfo.ArgAt<Claim>(0).UpdateSnapshots.FirstOrDefault().ProcessedByFusionDateTime);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            var converted = processedDateTime.HasValue ? processedDateTime.Value.UtcDateTime : DateTime.MinValue;

            Assert.Equal(DateTime.UtcNow, converted, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task WhenConsumingUnprocessedReconciliationUpdate_It_CallsUpdateWithVolvoReconIdentity()
        {
            var reconciliationUpdate = CreateReconciliationUpdate(_claim);
            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, reconciliationUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            SetupAddToUpdateSnapshotHistory(reconciliationUpdate);

            await _claimsService.Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoRecon.Username));

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _claimsService.Received(1).Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoRecon.Username));
        }

        [Fact]
        public async Task WhenConsumingDuplicateProcessedReconciliationUpdate_It_DoesNotCallUpdate()
        {
            var duplicateProcessedUpdate = CreateReconDuplicateUpdate(true);
            var claim = _claim;
            claim.UpdateSnapshots.Add(duplicateProcessedUpdate.UpdateSnapshot);

            var duplicateUpdate = CreateReconDuplicateUpdate();
            _claim.Id = duplicateUpdate.ClaimId;

            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, duplicateUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _claimsService.DidNotReceive().Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Any<FusionIdentity>());
        }

        [Fact]
        public async Task WhenConsumingDuplicateProcessedReconciliationUpdate_It_DoesNotCallTheFusionClient()
        {
            var duplicateProcessedUpdate = CreateReconDuplicateUpdate(true);
            var claim = _claim;
            claim.UpdateSnapshots.Add(duplicateProcessedUpdate.UpdateSnapshot);

            var duplicateUpdate = CreateReconDuplicateUpdate();
            _claim.Id = duplicateUpdate.ClaimId;

            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, duplicateUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _fusionClient.DidNotReceive()
                .ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenConsumingDuplicateUnprocessedReconciliationUpdate_It_DoesCallTheFusionClient()
        {
            var duplicateUnprocessedUpdate = CreateReconDuplicateUpdate();
            var claim = _claim;
            claim.UpdateSnapshots.Add(duplicateUnprocessedUpdate.UpdateSnapshot);

            var duplicateUpdate = CreateReconDuplicateUpdate();
            _claim.Id = duplicateUpdate.ClaimId;

            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, duplicateUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _fusionClient.Received(1)
                .ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenConsumingDuplicateUnprocessedReconciliationUpdate_It_DoesCallUpdate()
        {
            var duplicateUnprocessedUpdate = CreateReconDuplicateUpdate();
            var claim = _claim;
            claim.UpdateSnapshots.Add(duplicateUnprocessedUpdate.UpdateSnapshot);

            var duplicateUpdate = CreateReconDuplicateUpdate();
            _claim.Id = duplicateUpdate.ClaimId;

            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, duplicateUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _claimsService.Received(1)
                .Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Is<FusionIdentity>(x => x.Username == FusionIdentity.VolvoRecon.Username));
        }

        [Fact]
        public async Task WhenConsumingStatusUpdate_It_DoesNotCallTheFusionClient()
        {
            var statusUpdate = CreateStatusUpdate(_claim);
            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, statusUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _fusionClient.DidNotReceive()
                .ProcessWarrantyPaymentAsync(Arg.Any<WarrantyPaymentInformation>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenConsumingStatusUpdate_It_DoesNotCallUpdate()
        {
            var statusUpdate = CreateStatusUpdate(_claim);
            var consumeMessageContext = new MessageConsumeContext<UpdateSnapshotMessage>(_consumeContext, statusUpdate);
            var consumer = new UpdateSnapshotConsumer(_logger, _claimsService, _fusionClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => consumer.Consume(consumeMessageContext));

            await _claimsService.DidNotReceive().Update(Arg.Any<Claim>(), Arg.Any<string>(), Arg.Any<FusionIdentity>());
        }

        private UpdateSnapshotMessage CreateReconciliationUpdate(Claim claim, bool processed = false)
        {
            var update = AutoFaker.Generate<UpdateSnapshotMessage>();
            update.ClaimId = claim.Id;
            update.UpdateSnapshot.Type = UpdateType.Reconciliation;
            update.UpdateSnapshot.ProcessedByFusionDateTime = processed ? DateTimeOffset.Now : (DateTimeOffset?)null;
            return update;
        }

        private static UpdateSnapshotMessage CreateStatusUpdate(Claim claim)
        {
            var update = AutoFaker.Generate<UpdateSnapshotMessage>();
            update.ClaimId = claim.Id;
            update.UpdateSnapshot.Type = UpdateType.Status;
            return update;
        }

        private static Claim CreateClaimWithUpdate(UpdateSnapshotMessage update)
        {
            var claim = AutoFaker.Generate<Claim>();
            var updates = new List<UpdateSnapshot> { update.UpdateSnapshot };
            claim.Id = update.ClaimId;
            claim.UpdateSnapshots = updates;
            return claim;
        }

        private UpdateSnapshotMessage CreateReconDuplicateUpdate(bool processed = false) =>
            new UpdateSnapshotMessage
            {
                ClaimId = "12345",
                UpdateSnapshot = new UpdateSnapshot
                {
                    Type = UpdateType.Reconciliation,
                    Id = "123",
                    DealerCode = "dealerCode",
                    ProcessedByFusionDateTime = processed ? DateTimeOffset.MaxValue : (DateTimeOffset?)null,
                    RepairOrderNumber = "99",
                    ProcessDate = DateTime.MaxValue,
                    ApprovedAmount = new Money
                    {
                        Value = 10,
                        Currency = CurrencyCode.USD
                    }
                }
            };
    }
}
