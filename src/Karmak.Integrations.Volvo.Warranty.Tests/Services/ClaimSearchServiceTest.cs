using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;
using Karmak.Integrations.Volvo.Warranty.Persistence;
using Karmak.Integrations.Volvo.Warranty.Services;
using NSubstitute;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Services
{
    /// <summary>
    /// The service is where a request written by a caller becomes a query, so these are about what
    /// it accepts, what it turns away, and what it fills in when the request is quiet.
    /// </summary>
    public class ClaimSearchServiceTest
    {
        private static readonly Guid InstanceId = Guid.NewGuid();
        private static readonly Guid BranchId = Guid.NewGuid();

        private readonly ElkContext _elkContext;
        private readonly IClaimRepository _repository = Substitute.For<IClaimRepository>();
        private readonly ClaimSearchService _service;

        public ClaimSearchServiceTest()
        {
            _elkContext = Context(InstanceId, BranchId);

            _repository.Search(Arg.Any<WarrantyClaimSearchQuery>())
                .Returns(new ClaimSearchResponse());

            _service = new ClaimSearchService(_repository);
        }

        private static ElkContext Context(Guid? instance, Guid? branch)
        {
            var builder = new ElkContext.Builder
            {
                Identity = new ElkIdentity.Builder { Account = Guid.NewGuid(), User = Guid.NewGuid() }.Build(),
                SecurityProfile = new ElkIdentity.Builder { Account = Guid.NewGuid(), User = Guid.NewGuid() }.Build()
            };

            if (instance.HasValue || branch.HasValue)
            {
                builder.ApplicationContext = new ElkApplicationContext.Builder
                {
                    Instance = instance,
                    Branch = branch
                }.Build();
            }

            return builder.Build();
        }

        private Task<ClaimSearchResponse> Search(ClaimSearchRequest request) =>
            ImplicitElkContext.WithCurrentAsync(_elkContext, () => _service.Search(request));

        private WarrantyClaimSearchQuery CapturedQuery()
        {
            var call = _repository.ReceivedCalls()
                .Single(c => c.GetMethodInfo().Name == nameof(IClaimRepository.Search));

            return (WarrantyClaimSearchQuery)call.GetArguments()[0]!;
        }

        private static async Task<string> Rejected(Func<Task> search) =>
            (await Assert.ThrowsAsync<InvalidClaimSearchException>(search)).Message;

        #region Scope

        /// <summary>
        /// A caller searches the dealer and branch they signed in as. Nothing in the request can
        /// widen that, which is why neither is on the request at all.
        /// </summary>
        [Fact]
        public async Task It_SearchesTheCallersOwnInstanceAndBranch()
        {
            await Search(new ClaimSearchRequest { Keyword = "291" });

            var query = CapturedQuery();

            Assert.Equal(InstanceId.ToString(), query.InstanceIdentifier);
            Assert.Equal(BranchId.ToString(), query.BranchIdentifier);
        }

        [Fact]
        public async Task WhenThereIsNoElkInstance_It_Throws()
        {
            var exception = await Assert.ThrowsAsync<InvalidClaimContextException>(() =>
                ImplicitElkContext.WithCurrentAsync(
                    Context(null, null),
                    () => _service.Search(new ClaimSearchRequest { Keyword = "291" })));

            Assert.Equal("Invalid claim context: Elk Instance is missing.", exception.Message);
        }

        #endregion

        #region Modes

        [Fact]
        public async Task WhenGivenAKeyword_It_SearchesForIt()
        {
            await Search(new ClaimSearchRequest { Keyword = "  291  " });

            var query = CapturedQuery();

            Assert.Equal("291", query.Keyword);
            Assert.Empty(query.Terms);
        }

        [Fact]
        public async Task WhenGivenTerms_It_ReadsEachOneAsItsFieldsOwnType()
        {
            await Search(new ClaimSearchRequest
            {
                Terms =
                [
                    new ClaimSearchTerm { Field = "repairOrderIdentifier", Operator = "contains", Value = "291" },
                    new ClaimSearchTerm { Field = "claimTotal", Operator = "greaterThan", Value = "100.50" },
                    new ClaimSearchTerm { Field = "repairOrderOpenedDate", Operator = "lessThan", Value = "2026-03-02" }
                ]
            });

            var terms = CapturedQuery().Terms;

            Assert.Null(CapturedQuery().Keyword);
            Assert.Equal(WarrantyClaimField.RepairOrderIdentifier, terms[0].Field);
            Assert.Equal(WarrantyClaimSearchOperator.Contains, terms[0].Operator);
            Assert.Equal("291", terms[0].Value);
            Assert.Equal(100.50m, terms[1].Value);
            Assert.Equal(new DateTime(2026, 3, 2), terms[2].Value);
        }

        [Fact]
        public async Task It_ReadsFieldsAndOperatorsInAnyCasing()
        {
            await Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "CompanyName", Operator = "BEGINSWITH", Value = "ACME" }]
            });

            var term = Assert.Single(CapturedQuery().Terms);

            Assert.Equal(WarrantyClaimField.CompanyName, term.Field);
            Assert.Equal(WarrantyClaimSearchOperator.BeginsWith, term.Operator);
        }

        [Fact]
        public async Task WhenGivenBothModes_It_Rejects()
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Keyword = "291",
                Terms = [new ClaimSearchTerm { Field = "claimStatus", Operator = "equalTo", Value = "Submitted" }]
            }));

            Assert.Contains("not both", message);
        }

        [Fact]
        public async Task WhenGivenNeitherMode_It_ReturnsThePageUnfiltered()
        {
            await Search(new ClaimSearchRequest());

            var query = CapturedQuery();

            Assert.Null(query.Keyword);
            Assert.Empty(query.Terms);
        }

        [Fact]
        public async Task WhenThereIsNoRequest_It_Rejects()
        {
            await Assert.ThrowsAsync<InvalidClaimSearchException>(() => Search(null!));
        }

        #endregion

        #region What it turns away

        [Fact]
        public async Task WhenAFieldDoesNotExist_It_SaysWhichOnesDo()
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "jsonData", Operator = "contains", Value = "x" }]
            }));

            Assert.Contains("'jsonData' is not a field", message);
            Assert.Contains("repairOrderIdentifier", message);
        }

        /// <summary>
        /// Enum.TryParse would read a number as whichever member sits at that value, handing back a
        /// field the caller never named.
        /// </summary>
        [Fact]
        public async Task WhenAFieldIsANumber_It_Rejects()
        {
            await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "3", Operator = "contains", Value = "x" }]
            }));
        }

        [Fact]
        public async Task WhenAnOperatorDoesNotExist_It_SaysWhichOnesDo()
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "companyName", Operator = "soundsLike", Value = "ACME" }]
            }));

            Assert.Contains("'soundsLike' is not a way to match", message);
            Assert.Contains("beginsWith", message);
        }

        [Theory]
        [InlineData("contains")]
        [InlineData("beginsWith")]
        public async Task WhenMatchingTextAgainstANumber_It_Rejects(string @operator)
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "claimTotal", Operator = @operator, Value = "100" }]
            }));

            Assert.Contains("holds a number", message);
        }

        [Theory]
        [InlineData("claimTotal", "ten", "holds a number")]
        [InlineData("repairOrderOpenedDate", "the third", "holds a date")]
        public async Task WhenAValueIsNotTheFieldsType_It_SaysSo(string field, string value, string expected)
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = field, Operator = "equalTo", Value = value }]
            }));

            Assert.Contains(expected, message);
        }

        [Fact]
        public async Task WhenATermHasNoValue_It_Rejects()
        {
            var message = await Rejected(() => Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "companyName", Operator = "equalTo", Value = null }]
            }));

            Assert.Contains("nothing to match", message);
        }

        /// <summary>
        /// Terms are ANDed, and a field takes as many of them as the caller writes, which is how a
        /// range over one field is asked for.
        /// </summary>
        [Fact]
        public async Task WhenAFieldIsSearchedTwice_It_KeepsBothTerms()
        {
            await Search(new ClaimSearchRequest
            {
                Terms =
                [
                    new ClaimSearchTerm { Field = "claimTotal", Operator = "greaterThan", Value = "100" },
                    new ClaimSearchTerm { Field = "claimTotal", Operator = "lessThan", Value = "500" }
                ]
            });

            var terms = CapturedQuery().Terms;

            Assert.Equal(2, terms.Count);
            Assert.Equal(WarrantyClaimField.ClaimTotal, terms[0].Field);
            Assert.Equal(WarrantyClaimSearchOperator.GreaterThan, terms[0].Operator);
            Assert.Equal(100m, terms[0].Value);
            Assert.Equal(WarrantyClaimField.ClaimTotal, terms[1].Field);
            Assert.Equal(WarrantyClaimSearchOperator.LessThan, terms[1].Operator);
            Assert.Equal(500m, terms[1].Value);
        }

        #endregion

        #region Paging and sorting

        [Fact]
        public async Task WhenNoPageIsAskedFor_It_ReturnsTheFirstOne()
        {
            await Search(new ClaimSearchRequest());

            var query = CapturedQuery();

            Assert.Equal(0, query.Skip);
            Assert.Equal(ClaimSearchService.DefaultTop, query.Take);
        }

        [Fact]
        public async Task WhenAPageIsAskedFor_It_ReturnsThatOne()
        {
            await Search(new ClaimSearchRequest { Skip = 100, Top = 25 });

            var query = CapturedQuery();

            Assert.Equal(100, query.Skip);
            Assert.Equal(25, query.Take);
        }

        /// <summary>
        /// Trimmed rather than refused, so a caller reaching for everything still gets an answer.
        /// </summary>
        [Fact]
        public async Task WhenTooLargeAPageIsAskedFor_It_Trims()
        {
            await Search(new ClaimSearchRequest { Top = 5000 });

            Assert.Equal(ClaimSearchService.MaximumTop, CapturedQuery().Take);
        }

        [Theory]
        [InlineData(-1, 50)]
        [InlineData(0, -1)]
        public async Task WhenThePageMakesNoSense_It_Rejects(int skip, int top)
        {
            await Rejected(() => Search(new ClaimSearchRequest { Skip = skip, Top = top }));
        }

        [Fact]
        public async Task WhenNoOrderIsAskedFor_It_LeavesTheDefaultInPlace()
        {
            await Search(new ClaimSearchRequest { Descending = false });

            Assert.Null(CapturedQuery().SortField);
        }

        [Fact]
        public async Task WhenAnOrderIsAskedFor_It_SortsByIt()
        {
            await Search(new ClaimSearchRequest { OrderBy = "companyName", Descending = true });

            var query = CapturedQuery();

            Assert.Equal(WarrantyClaimField.CompanyName, query.SortField);
            Assert.True(query.SortDescending);
        }

        [Fact]
        public async Task WhenTheOrderNamesNothing_It_Rejects()
        {
            await Rejected(() => Search(new ClaimSearchRequest { OrderBy = "whatever" }));
        }

        [Fact]
        public async Task It_LeavesDeletedClaimsOutUnlessAskedFor()
        {
            await Search(new ClaimSearchRequest());
            Assert.False(CapturedQuery().IncludeDeleted);

            _repository.ClearReceivedCalls();

            await Search(new ClaimSearchRequest { IncludeDeleted = true });
            Assert.True(CapturedQuery().IncludeDeleted);
        }

        #endregion

        #region New only

        /// <summary>
        /// A flag rather than a term, so it narrows a keyword sweep too. That is what the ui asks for:
        /// a search across every field, restricted to the claims still waiting to be sent.
        /// </summary>
        [Fact]
        public async Task WhenAskedForTheNewClaimsOnly_It_NarrowsAKeywordSweep()
        {
            await Search(new ClaimSearchRequest { Keyword = "291", NewOnly = true });

            var query = CapturedQuery();

            Assert.True(query.NewOnly);
            Assert.Equal("291", query.Keyword);
        }

        [Fact]
        public async Task It_KeepsEveryStatusUnlessTheNewOnesAreAskedFor()
        {
            await Search(new ClaimSearchRequest());

            Assert.False(CapturedQuery().NewOnly);
        }

        #endregion

        #region Days and moments

        /// <summary>
        /// Anyone filtering on a date writes the day and means all of it. Naming a time is how a
        /// caller says they meant one moment, and the term carries which of the two they wrote.
        /// </summary>
        [Theory]
        [InlineData("2026-03-02", true)]
        [InlineData("  2026-03-02  ", true)]
        [InlineData("3/2/2026", true)]
        [InlineData("2026-03-02T14:30:00Z", false)]
        [InlineData("2026-03-02 14:30", false)]
        public async Task It_TellsADayFromAMomentWithinOne(string value, bool expected)
        {
            await Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "repairOrderOpenedDate", Operator = "equalTo", Value = value }]
            });

            Assert.Equal(expected, Assert.Single(CapturedQuery().Terms).DateOnly);
        }

        [Fact]
        public async Task WhenAFieldHoldsNoDate_It_IsNeverADay()
        {
            await Search(new ClaimSearchRequest
            {
                Terms = [new ClaimSearchTerm { Field = "companyName", Operator = "equalTo", Value = "ACME" }]
            });

            Assert.False(Assert.Single(CapturedQuery().Terms).DateOnly);
        }

        #endregion
    }
}
