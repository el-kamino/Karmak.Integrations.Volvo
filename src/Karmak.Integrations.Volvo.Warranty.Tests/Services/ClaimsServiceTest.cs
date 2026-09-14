using Karmak.Integrations.Volvo.Warranty.Persistence;
using AutoBogus;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Settings;

using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Services;
using Karmak.Integrations.Volvo.Warranty.Services.Interfaces;
using Karmak.Integrations.Volvo.Warranty.Storage;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using Karmak.Integrations.Volvo.Warranty.Translators.SubmitClaim;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Services
{
    public class ClaimsServiceTest
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IBusControl _bus;
        private readonly ISettingsProvider _settingsClient;
        private readonly ILogger<ClaimsService> _logger;
        private readonly IWarrantyTableClient _tableClient;
        private readonly ClaimsService _claimsService;
        private readonly Claim _fakeClaim;
        private readonly FusionIdentity _fakeFusionIdentity;
        private readonly ElkContext _fakeElkContext;
        private readonly VolvoSettings _fakeVolvoSettings;

        public ClaimsServiceTest()
        {
            _fakeFusionIdentity = new FusionIdentity
            {
                AccountCode = "12345",
                BranchCode = "01",
                Username = "eddieVanHalen"
            };

            _fakeElkContext = new ElkContext.Builder
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
                }.Build(),
                ApplicationContext = new ElkApplicationContext.Builder
                {
                    Instance = Guid.NewGuid(),
                    Branch = Guid.NewGuid()
                }.Build()
            }.Build();

            _fakeClaim = FakeClaim.Generate();
            _fakeVolvoSettings = AutoFaker.Generate<VolvoSettings>();

            _claimRepository = Substitute.For<IClaimRepository>();
            _bus = Substitute.For<IBusControl>();
            _settingsClient = Substitute.For<ISettingsProvider>();
            _logger = Substitute.For<ILogger<ClaimsService>>();
            _tableClient = Substitute.For<IWarrantyTableClient>();

            _claimRepository.Create(Arg.Any<Claim>())
                .Returns(ci => ci.ArgAt<Claim>(0));

            _claimRepository.Update(Arg.Any<Claim>())
                .Returns(ci => ci.ArgAt<Claim>(0));

            _claimRepository.Find(Arg.Any<string>(), Arg.Any<string>())
                .Returns(_fakeClaim);

            _claimRepository.FindActiveByWarrantyRepairOrder(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Enumerable.Empty<Claim>());

            _claimRepository.FindActiveByRepairOrder(Arg.Any<string>(), Arg.Any<RepairOrderKey>())
                .Returns(Enumerable.Empty<Claim>());

            _claimRepository.FindActiveInCorrelation(Arg.Any<string>(), Arg.Any<RepairOrderKey>(), Arg.Any<string>())
                .Returns(Enumerable.Empty<Claim>());

            _claimRepository.FindActiveOutsideCorrelation(Arg.Any<string>(), Arg.Any<RepairOrderKey>(), Arg.Any<string>())
                .Returns(Enumerable.Empty<Claim>());

            _settingsClient.GetSettingsAsync()
                .Returns(_fakeVolvoSettings);

            _claimsService = new ClaimsService(
                _claimRepository,
                _bus,
                _settingsClient,
                _logger,
                _tableClient);
        }

        #region Create()

        [Fact]
        public async Task WhenCreating_It_DelegatesToRepository()
        {
            var claim = FakeClaim.Generate();

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(claim, _fakeFusionIdentity));

            await _claimRepository.Received(1).Create(Arg.Is<Claim>(c => c.Id == claim.Id));
        }

        [Fact]
        public async Task WhenCreating_It_SetsClaimId()
        {
            var claim = FakeClaim.Generate();
            var originalId = claim.Id;

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(claim, _fakeFusionIdentity));

            Assert.NotEqual(originalId, result.Id);
            Assert.NotNull(result.Id);
        }

        [Fact]
        public async Task WhenCreating_It_SetsCreatedAndUpdatedTimesToCurrentTime()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(FakeClaim.Generate(), _fakeFusionIdentity));

            Assert.Equal(DateTime.UtcNow, result.UpdatedDateTime.UtcDateTime, TimeSpan.FromSeconds(1));
            Assert.Equal(DateTime.UtcNow, result.CreatedDateTime.UtcDateTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task WhenCreating_It_SetsCreatedAndUpdatedByToFusionIdentityUsername()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(FakeClaim.Generate(), _fakeFusionIdentity));

            Assert.Equal(_fakeFusionIdentity.Username, result.CreatedBy);
            Assert.Equal(_fakeFusionIdentity.Username, result.UpdatedBy);
        }

        [Fact]
        public async Task WhenCreating_It_SetsApplicationAndBranchCodesToFusionCodes()
        {
            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(FakeClaim.Generate(), _fakeFusionIdentity));

            await _claimRepository.Received(1).Create(Arg.Is<Claim>(claim =>
                claim.Dealer.Account.Code == _fakeFusionIdentity.AccountCode
                && claim.Dealer.Branch.Code == _fakeFusionIdentity.BranchCode));
        }

        [Fact]
        public async Task WhenCreating_It_SetsAccountToElkAccount()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(FakeClaim.Generate(), _fakeFusionIdentity));

            Assert.Equal(_fakeElkContext.Identity.Account.ToString(), result.Dealer.Account.Identifier);
        }

        [Fact]
        public async Task WhenCreating_It_SetsBranchIdToElkBranchIdentifier()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(FakeClaim.Generate(), _fakeFusionIdentity));

            Assert.Equal(_fakeElkContext.ApplicationContext.Branch.ToString(), result.Dealer.Branch.Identifier);
        }

        [Fact]
        public async Task WhenCreatingForAnExistingClaimMatch_It_Throws()
        {
            var claim = FakeClaim.Generate();
            claim.CorrelationId = null;

            _claimRepository.FindActiveByRepairOrder(Arg.Any<string>(), Arg.Any<RepairOrderKey>())
                .Returns(new List<Claim> { claim });

            var exception = await Assert.ThrowsAsync<ClaimAlreadyExistsException>(() =>
                ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Create(claim, _fakeFusionIdentity)));

            Assert.Contains("An active claim already exists matching the criteria", exception.Message);
        }

        #endregion

        #region Find()

        [Fact]
        public async Task WhenFinding_It_DelegatesToRepository()
        {
            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Find(_fakeClaim.Id));

            await _claimRepository.Received(1).Find(
                Arg.Is<string>(v => v == _fakeElkContext.ApplicationContext.Instance.ToString()),
                Arg.Is<string>(v => v == _fakeClaim.Id));
        }

        [Fact]
        public async Task WhenFindingWithMissingElkInstance_It_Throws()
        {
            var badElkContext = new ElkContext.Builder
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

            var exception = await Assert.ThrowsAsync<InvalidClaimContextException>(() =>
                ImplicitElkContext.WithCurrentAsync(badElkContext, () => _claimsService.Find(_fakeClaim.Id)));

            Assert.Equal("Invalid claim context: Elk Instance is missing.", exception.Message);
        }

        #endregion

        #region FindByRepairOrderId()

        [Fact]
        public async Task WhenFindingByRepairOrderId_It_ReturnsAllClaimsWithMatchingRepairOrderId()
        {
            var claimToBeFound = FakeClaim.Generate();
            claimToBeFound.IsDeleted = false;
            var fakeQueryResults = new List<Claim> { claimToBeFound, claimToBeFound };

            _claimRepository.FindActiveByWarrantyRepairOrder(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(fakeQueryResults);

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.FindByRepairOrderId(claimToBeFound.RepairOrder.SecondaryIdentifier));

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task WhenFindingByRepairOrderId_It_FiltersOutDeletedClaims()
        {
            _claimRepository.FindActiveByWarrantyRepairOrder(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new List<Claim>());

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.FindByRepairOrderId(Guid.NewGuid().ToString()));

            Assert.Empty(result);
        }

        [Fact]
        public async Task WhenFindingByRepairOrderId_It_DelegatesToTheRepository()
        {
            var claimToBeFound = FakeClaim.Generate();

            _claimRepository.FindActiveByWarrantyRepairOrder(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new List<Claim> { claimToBeFound });

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.FindByRepairOrderId(claimToBeFound.RepairOrder.SecondaryIdentifier));

            await _claimRepository.Received(1).FindActiveByWarrantyRepairOrder(
                _fakeElkContext.ApplicationContext.Instance.ToString(),
                _fakeElkContext.ApplicationContext.Branch.ToString(),
                claimToBeFound.RepairOrder.SecondaryIdentifier);
        }

        [Fact]
        public async Task WhenFindingByRepairOrderIdWithMissingElkInstance_It_Throws()
        {
            var badElkContext = new ElkContext.Builder
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
                }.Build(),
                ApplicationContext = new ElkApplicationContext.Builder
                {
                    Branch = Guid.NewGuid()
                }.Build()
            }.Build();

            var exception = await Assert.ThrowsAsync<InvalidClaimContextException>(() =>
                ImplicitElkContext.WithCurrentAsync(badElkContext, () => _claimsService.FindByRepairOrderId(Guid.NewGuid().ToString())));

            Assert.Equal("Invalid claim context: Elk Instance is missing.", exception.Message);
        }

        #endregion

        #region Delete()

        [Fact]
        public async Task WhenDeleting_It_SetsIsDeletedToTrue()
        {
            _fakeClaim.CorrelationId = null;

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Delete(_fakeClaim.Id, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(v => v.IsDeleted && v.Id == _fakeClaim.Id));
        }

        [Fact]
        public async Task WhenDeleting_It_SetsUpdatedByToFusionIdentityUsername()
        {
            _fakeClaim.CorrelationId = null;

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Delete(_fakeClaim.Id, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(v => v.UpdatedBy == _fakeFusionIdentity.Username && v.Id == _fakeClaim.Id));
        }

        #endregion

        #region Submit()

        [Fact]
        public async Task WhenSubmitting_It_DelegatesToBusSend()
        {
            var message = FakeClaim.Generate();
            message.CorrelationId = null;

            await _claimsService.Submit(message, TestContext.Current.CancellationToken);
            await _bus.Received(1).Publish(Arg.Any<SubmitClaimPayload>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region Update()

        [Fact]
        public async Task WhenUpdating_It_DelegatesToRepository()
        {
            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(_fakeClaim, _fakeClaim.Id, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(v => v.Id == _fakeClaim.Id));
        }

        [Fact]
        public async Task WhenUpdating_It_SetsUpdatedTimeToCurrentTime()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(_fakeClaim, _fakeClaim.Id, _fakeFusionIdentity));

            Assert.Equal(DateTime.UtcNow, result.UpdatedDateTime.UtcDateTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task WhenUpdating_It_RetainsTheOriginalClaimId()
        {
            var originalClaimId = _fakeClaim.Id;
            var updatedClaim = CreateUpdatedClaim(_fakeClaim);

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(updatedClaim, originalClaimId, _fakeFusionIdentity));

            Assert.Equal(originalClaimId, result.Id);
        }

        [Fact]
        public async Task WhenUpdating_It_UpdatesPartIds()
        {
            var originalClaimId = _fakeClaim.Id;
            var updatedClaim = CreateUpdatedClaim(_fakeClaim);
            var mockPart = updatedClaim.RepairOrder.Jobs.First().PartExpenses.First();
            mockPart.IsCausalPart = true;

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(updatedClaim, originalClaimId, _fakeFusionIdentity));

            var expectedPartId = $"{mockPart.Prefix}{mockPart.Number}{mockPart.Suffix}";
            Assert.Equal(expectedPartId, result.RepairOrder.Jobs.First().PartExpenses.First().Identifier);
        }

        [Fact]
        public async Task WhenUpdatingWithOutdatedUpdate_It_ReturnsNull()
        {
            var updatedClaim = CreateUpdatedClaim(_fakeClaim, true);

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(updatedClaim, updatedClaim.Id, _fakeFusionIdentity));

            Assert.Null(result);
        }

        [Fact]
        public async Task WhenUpdating_It_RetainsOriginalCreatedByAndDateTime()
        {
            var updatedClaim = CreateUpdatedClaim(_fakeClaim);
            var originalCreatedBy = _fakeClaim.CreatedBy;
            var originalCreatedDateTime = _fakeClaim.CreatedDateTime;

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(updatedClaim, _fakeClaim.Id, _fakeFusionIdentity));

            Assert.Equal(originalCreatedBy, result.CreatedBy);
            Assert.Equal(originalCreatedDateTime, result.CreatedDateTime);
        }

        [Fact]
        public async Task WhenUpdating_It_SetsUpdatedByToFusionIdentityUsername()
        {
            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(_fakeClaim, _fakeClaim.Id, _fakeFusionIdentity));

            Assert.Equal(_fakeFusionIdentity.Username, result.UpdatedBy);
        }

        [Fact]
        public async Task WhenUpdating_It_RetainsOriginalDealerInfo()
        {
            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => _claimsService.Update(_fakeClaim, _fakeClaim.Id, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(claim =>
                claim.Dealer.InstanceIdentifier == _fakeClaim.Dealer.InstanceIdentifier
                && claim.Dealer.Account.Identifier == _fakeClaim.Dealer.Account.Identifier
                && claim.Dealer.Account.Code == _fakeClaim.Dealer.Account.Code
                && claim.Dealer.Branch.Identifier == _fakeClaim.Dealer.Branch.Identifier
                && claim.Dealer.Branch.Code == _fakeClaim.Dealer.Branch.Code));
        }

        [Fact]
        public async Task WhenUpdating_It_SavesOemUserIDFromSettingsIfMapped()
        {
            _fakeVolvoSettings.InterfaceOptions.OemUserMappings = new Dictionary<string, string> { { "stubUsername", "stubOemUserID" } };
            _fakeClaim.RepairOrder.Advisor.Username = "stubUsername";

            _settingsClient.GetSettingsAsync()
                .Returns(_fakeVolvoSettings);

            var service = new ClaimsService(
                _claimRepository, _bus, _settingsClient, _logger, _tableClient);

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () => service.Update(_fakeClaim, _fakeClaim.Id, _fakeFusionIdentity));

            await _claimRepository.Received().Update(Arg.Is<Claim>(claim =>
                claim.RepairOrder.Advisor.Identifier == "stubOemUserID"));
        }

        #endregion

        #region UpdateStatus()

        [Fact]
        public async Task WhenUpdatingStatus_It_FindsClaimAndUpdatesStatus()
        {
            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.UpdateStatus(_fakeClaim.Id, ClaimStatus.Submitted, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(c => c.Status == ClaimStatus.Submitted));
        }

        #endregion

        #region AddToUpdateSnapshotHistory()

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistory_It_DelegatesToRepository()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();

            await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            await _claimRepository.Received(1).Update(Arg.Is<Claim>(v => v.Id == _fakeClaim.Id));
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistoryWithUniqueUpdate_It_AddsToClaimsUpdateSnapshots()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();
            _fakeClaim.UpdateSnapshots = new List<UpdateSnapshot> { AutoFaker.Generate<UpdateSnapshot>() };

            var updatedClaim = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(updatedClaim);
            Assert.Equal(2, updatedClaim.UpdateSnapshots.Count());
            Assert.Contains(fakeUpdateSnapshot, updatedClaim.UpdateSnapshots);
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistoryWithNonuniqueUpdate_It_DoesNotAddTheUpdateSnapshot()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();
            _fakeClaim.UpdateSnapshots = new List<UpdateSnapshot> { fakeUpdateSnapshot };

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(result);
            Assert.Single(result.UpdateSnapshots);
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistoryWithNoUpdateSnapshots_It_AddsUpdateSnapshot()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();

            var updatedClaim = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(updatedClaim);
            Assert.Single(updatedClaim.UpdateSnapshots);
            Assert.Contains(fakeUpdateSnapshot, updatedClaim.UpdateSnapshots);
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistory_It_UpdatesClaimStatus()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();
            fakeUpdateSnapshot.Type = UpdateType.Status;

            var updatedClaim = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(updatedClaim);
            Assert.Equal(fakeUpdateSnapshot.Status.Description, updatedClaim.Status.Value);
            Assert.Equal(fakeUpdateSnapshot.Status.Code, updatedClaim.Status.Code);
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistory_It_SetsUpdatedTimeToCurrentUTCTime()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(result);
            Assert.Equal(DateTime.UtcNow, result.UpdatedDateTime.UtcDateTime, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task WhenAddingToUpdateSnapshotHistory_It_SetsUpdatedByToFusionIdentityUsername()
        {
            var fakeUpdateSnapshot = AutoFaker.Generate<UpdateSnapshot>();

            var result = await ImplicitElkContext.WithCurrentAsync(_fakeElkContext, () =>
                _claimsService.AddToUpdateSnapshotHistory(_fakeClaim.Id, fakeUpdateSnapshot, _fakeFusionIdentity));

            Assert.NotNull(result);
            Assert.Equal(_fakeFusionIdentity.Username, result.UpdatedBy);
        }

        #endregion

        private static Claim CreateUpdatedClaim(Claim originalClaim, bool isOutdated = false)
        {
            var claim = AutoFaker.Generate<Claim>();
            claim.Id = originalClaim.Id;
            claim.UpdatedDateTime = isOutdated
                ? originalClaim.UpdatedDateTime.Subtract(TimeSpan.FromDays(1))
                : originalClaim.UpdatedDateTime;
            return claim;
        }
    }
}
