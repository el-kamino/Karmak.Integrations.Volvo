using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Models;

using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Mapping;
using Karmak.Integrations.Volvo.Warranty.Persistence;
using Karmak.Integrations.Volvo.Warranty.Tests.Fakes;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Persistence
{
    public class SqlClaimRepositoryTest
    {
        private const string Instance = "instance-1";
        private const string Branch = "branch-1";

        private static readonly RepairOrderKey RepairOrder = new(Branch, "RO-1", "WRO-1");

        private readonly IWarrantyDataRepository _dataRepository = Substitute.For<IWarrantyDataRepository>();
        private readonly IWarrantyClaimEntityMapper _mapper = new WarrantyClaimEntityMapper();
        private readonly SqlClaimRepository _repository;

        public SqlClaimRepositoryTest()
        {
            _dataRepository.QueryAsync(Arg.Any<WarrantyClaimQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<WarrantyClaimEntity>());

            _dataRepository.SearchAsync(Arg.Any<WarrantyClaimSearchQuery>(), Arg.Any<CancellationToken>())
                .Returns(new WarrantyClaimSearchPage());

            _repository = new SqlClaimRepository(_dataRepository, _mapper);
        }

        private WarrantyClaimQuery CapturedQuery()
        {
            var call = _dataRepository.ReceivedCalls().Single(c => c.GetMethodInfo().Name == nameof(IWarrantyDataRepository.QueryAsync));

            return (WarrantyClaimQuery)call.GetArguments()[0];
        }

        #region Create() / Update()

        [Fact]
        public async Task WhenCreating_It_UpsertsThePromotedRow()
        {
            var claim = FakeClaim.Generate();

            var result = await _repository.Create(claim);

            await _dataRepository.Received(1).UpsertAsync(
                Arg.Is<WarrantyClaimEntity>(e =>
                    e.ClaimId == claim.Id
                    && e.InstanceIdentifier == claim.Dealer.InstanceIdentifier
                    && e.JsonData != null),
                Arg.Any<CancellationToken>());

            Assert.Same(claim, result);
        }

        [Fact]
        public async Task WhenUpdating_It_UpsertsThePromotedRow()
        {
            var claim = FakeClaim.Generate();

            await _repository.Update(claim);

            await _dataRepository.Received(1).UpsertAsync(
                Arg.Is<WarrantyClaimEntity>(e => e.ClaimId == claim.Id),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenSavingAClaimWithNoInstance_It_ThrowsWithoutWriting()
        {
            var claim = FakeClaim.Generate();
            claim.Dealer.InstanceIdentifier = null;

            await Assert.ThrowsAsync<InvalidClaimContextException>(() => _repository.Create(claim));

            await _dataRepository.DidNotReceiveWithAnyArgs().UpsertAsync(default, TestContext.Current.CancellationToken);
        }

        #endregion

        #region Find()

        [Fact]
        public async Task WhenFinding_It_RehydratesTheClaimFromJson()
        {
            var claim = FakeClaim.Generate();

            _dataRepository.FindAsync(Instance, claim.Id, Arg.Any<CancellationToken>())
                .Returns(_mapper.ToEntity(claim));

            var result = await _repository.Find(Instance, claim.Id);

            Assert.NotNull(result);
            Assert.Equal(claim.Id, result.Id);
            Assert.Equal(claim.Identifier, result.Identifier);
        }

        [Fact]
        public async Task WhenFindingAMissingClaim_It_ReturnsNull()
        {
            _dataRepository.FindAsync(Instance, "nope", Arg.Any<CancellationToken>())
                .Returns((WarrantyClaimEntity)null);

            Assert.Null(await _repository.Find(Instance, "nope"));
        }

        #endregion

        #region Lookups

        [Fact]
        public async Task WhenFindingByWarrantyRepairOrder_It_FiltersOnBranchRepairOrderAndDeleted()
        {
            await _repository.FindActiveByWarrantyRepairOrder(Instance, Branch, "WRO-1");

            var query = CapturedQuery();

            Assert.Equal(Instance, query.InstanceIdentifier);
            Assert.Equal(Branch, query.BranchIdentifier);
            Assert.Equal("WRO-1", query.WarrantyRepairOrderIdentifier);
            Assert.False(query.IsDeleted);
            //This lookup spans repair orders and correlation groups
            Assert.Null(query.RepairOrderIdentifier);
            Assert.Null(query.CorrelationId);
        }

        [Fact]
        public async Task WhenFindingByRepairOrder_It_FiltersOnBothRepairOrderIdentifiers()
        {
            await _repository.FindActiveByRepairOrder(Instance, RepairOrder);

            var query = CapturedQuery();

            Assert.Equal(Branch, query.BranchIdentifier);
            Assert.Equal("RO-1", query.RepairOrderIdentifier);
            Assert.Equal("WRO-1", query.WarrantyRepairOrderIdentifier);
            Assert.False(query.IsDeleted);
            Assert.Null(query.CorrelationId);
        }

        [Fact]
        public async Task WhenFindingInCorrelation_It_MatchesTheCorrelationId()
        {
            await _repository.FindActiveInCorrelation(Instance, RepairOrder, "corr-A");

            var query = CapturedQuery();

            Assert.Equal("corr-A", query.CorrelationId);
            Assert.False(query.ExcludeCorrelationId);
            Assert.False(query.IsDeleted);
        }

        [Fact]
        public async Task WhenFindingOutsideCorrelation_It_ExcludesTheCorrelationId()
        {
            await _repository.FindActiveOutsideCorrelation(Instance, RepairOrder, "corr-A");

            var query = CapturedQuery();

            Assert.Equal("corr-A", query.CorrelationId);
            Assert.True(query.ExcludeCorrelationId);
            Assert.False(query.IsDeleted);
        }

        [Fact]
        public async Task WhenQuerying_It_ReturnsTheRehydratedClaims()
        {
            var claims = FakeClaim.Generate(3).ToList();

            _dataRepository.QueryAsync(Arg.Any<WarrantyClaimQuery>(), Arg.Any<CancellationToken>())
                .Returns(claims.Select(_mapper.ToEntity).ToList());

            var result = await _repository.FindActiveByRepairOrder(Instance, RepairOrder);

            Assert.Equal(claims.Select(c => c.Id), result.Select(c => c.Id));
        }

        [Fact]
        public async Task WhenQueryingAndARowHasNoPayload_It_SkipsThatRow()
        {
            var claim = FakeClaim.Generate();

            _dataRepository.QueryAsync(Arg.Any<WarrantyClaimQuery>(), Arg.Any<CancellationToken>())
                .Returns(new List<WarrantyClaimEntity>
                {
                    _mapper.ToEntity(claim),
                    new() { ClaimId = "empty-row" }
                });

            var result = await _repository.FindActiveByRepairOrder(Instance, RepairOrder);

            Assert.Equal(claim.Id, Assert.Single(result).Id);
        }

        [Fact]
        public async Task WhenGivenNoRepairOrder_It_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _repository.FindActiveByRepairOrder(Instance, null!));
        }

        #endregion

        #region Search()

        private static WarrantyClaimSearchQuery SearchQuery() => new()
        {
            InstanceIdentifier = Instance,
            BranchIdentifier = Branch,
            Keyword = "291",
            Skip = 20,
            Take = 10
        };

        [Fact]
        public async Task WhenSearching_It_AsksTheStoreTheQueryItWasGiven()
        {
            var query = SearchQuery();

            await _repository.Search(query);

            await _dataRepository.Received(1).SearchAsync(query, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task WhenSearching_It_ReturnsThePromotedColumnsAsResults()
        {
            _dataRepository.SearchAsync(Arg.Any<WarrantyClaimSearchQuery>(), Arg.Any<CancellationToken>())
                .Returns(new WarrantyClaimSearchPage
                {
                    TotalCount = 137,
                    Items =
                    [
                        new WarrantyClaimSummary
                        {
                            ClaimId = "claim-1",
                            ClaimIdentifier = "C-1001",
                            RepairOrderIdentifier = "291234",
                            CompanyName = "ACME Trucking",
                            ClaimTotal = 1450.00m,
                            ClaimStatus = "Submitted"
                        },
                        new WarrantyClaimSummary { ClaimId = "claim-2" }
                    ]
                });

            var response = await _repository.Search(SearchQuery());

            Assert.Equal(["claim-1", "claim-2"], response.Results.Select(r => r.ClaimId));

            var first = response.Results.First();
            Assert.Equal("C-1001", first.ClaimIdentifier);
            Assert.Equal("291234", first.RepairOrderIdentifier);
            Assert.Equal("ACME Trucking", first.CompanyName);
            Assert.Equal(1450.00m, first.ClaimTotal);
            Assert.Equal("Submitted", first.ClaimStatus);
        }

        /// <summary>
        /// The page a caller asked for comes back with it, so paging never needs a second call to
        /// work out where it is.
        /// </summary>
        [Fact]
        public async Task WhenSearching_It_ReportsTheWholeResultSetAlongsideThePage()
        {
            _dataRepository.SearchAsync(Arg.Any<WarrantyClaimSearchQuery>(), Arg.Any<CancellationToken>())
                .Returns(new WarrantyClaimSearchPage { TotalCount = 137 });

            var response = await _repository.Search(SearchQuery());

            Assert.Equal(137, response.TotalCount);
            Assert.Equal(20, response.Skip);
            Assert.Equal(10, response.Top);
            Assert.Empty(response.Results);
        }

        [Fact]
        public async Task WhenSearchingWithNoQuery_It_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.Search(null!));
        }

        #endregion
    }
}
