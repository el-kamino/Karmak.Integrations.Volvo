using System.Data;
using System.Text.RegularExpressions;
using Karmak.Integrations.Volvo.Common.Sql;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Microsoft.Data.SqlClient;

namespace Karmak.Integrations.Volvo.Warranty.Tests.Sql
{
    /// <summary>
    /// The builder is the only part of searching with anything to get wrong, and it is a pure
    /// function, so these run the real statement generation without a database anywhere near them.
    /// </summary>
    public class WarrantyClaimSearchSqlBuilderTest
    {
        private const string Instance = "instance-1";
        private const string Branch = "branch-1";

        private static WarrantyClaimSearchQuery Query(
            string keyword = null,
            IReadOnlyList<WarrantyClaimSearchTerm> terms = null,
            int skip = 0,
            int take = 50)
        {
            return new WarrantyClaimSearchQuery
            {
                InstanceIdentifier = Instance,
                BranchIdentifier = Branch,
                Keyword = keyword,
                Terms = terms,
                Skip = skip,
                Take = take
            };
        }

        private static WarrantyClaimSearchTerm Term(
            WarrantyClaimField field,
            WarrantyClaimSearchOperator @operator,
            object value)
        {
            return new WarrantyClaimSearchTerm { Field = field, Operator = @operator, Value = value };
        }

        private static WarrantyClaimSearchTerm Day(
            WarrantyClaimField field,
            WarrantyClaimSearchOperator @operator,
            DateTime day)
        {
            return new WarrantyClaimSearchTerm { Field = field, Operator = @operator, Value = day, DateOnly = true };
        }

        private static readonly DateTime SecondOfMarch = new(2026, 3, 2);
        private static readonly DateTime ThirdOfMarch = new(2026, 3, 3);

        private static bool HasParameter(WarrantyClaimSearchCommand command, string name) =>
            command.Parameters.Any(p => p.ParameterName == name);

        /// <summary>
        /// Collapses the statement's own layout so a test says what the sql means rather than how it
        /// happens to be wrapped.
        /// </summary>
        private static string Normalize(string sql) => Regex.Replace(sql, @"\s+", " ").Trim();

        private static object ValueOf(WarrantyClaimSearchCommand command, string name) =>
            command.Parameters.Single(p => p.ParameterName == name).Value;

        #region Scope and paging

        [Fact]
        public void It_ScopesEverySearchToTheInstanceAndBranch()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query());
            var sql = Normalize(command.Sql);

            Assert.Contains("WHERE [InstanceIdentifier] = @InstanceIdentifier", sql);
            Assert.Contains("AND (@BranchIdentifier IS NULL OR [BranchIdentifier] = @BranchIdentifier)", sql);
            Assert.Equal(Instance, ValueOf(command, "@InstanceIdentifier"));
            Assert.Equal(Branch, ValueOf(command, "@BranchIdentifier"));
        }

        [Fact]
        public void It_LeavesDeletedClaimsOutUnlessAskedFor()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query());

            Assert.Contains("AND (@IncludeDeleted = 1 OR [IsDeleted] = 0)", Normalize(command.Sql));
            Assert.Equal(false, ValueOf(command, "@IncludeDeleted"));

            var query = Query();
            query.IncludeDeleted = true;

            Assert.Equal(true, ValueOf(WarrantyClaimSearchSqlBuilder.Build(query), "@IncludeDeleted"));
        }

        [Fact]
        public void It_KeepsEveryStatusUnlessTheNewOnesAreAskedFor()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query());

            Assert.Contains("AND (@NewOnly = 0 OR [ClaimStatus] = @NewClaimStatus)", Normalize(command.Sql));
            Assert.Equal(false, ValueOf(command, "@NewOnly"));
            Assert.Equal("New", ValueOf(command, "@NewClaimStatus"));

            var query = Query();
            query.NewOnly = true;

            Assert.Equal(true, ValueOf(WarrantyClaimSearchSqlBuilder.Build(query), "@NewOnly"));
        }

        /// <summary>
        /// The search the ui opens on, and the one shape terms cannot express: a keyword sweeps every
        /// text column at once, so the status it is narrowed by cannot travel as a term beside it.
        /// </summary>
        [Fact]
        public void WhenGivenAKeywordAndNewOnly_It_NarrowsTheSweep()
        {
            var query = Query(keyword: "291");
            query.NewOnly = true;

            var sql = Normalize(WarrantyClaimSearchSqlBuilder.Build(query).Sql);

            Assert.Contains("[ClaimIdentifier] LIKE @Keyword", sql);
            Assert.Contains("AND (@NewOnly = 0 OR [ClaimStatus] = @NewClaimStatus)", sql);
        }

        /// <summary>
        /// The branch a claim was raised at, as a dealer names it. Every row in a branch-scoped search
        /// carries the same one, which is why it is read back rather than searched by.
        /// </summary>
        [Fact]
        public void It_ReadsTheBranchCodeWithThePage()
        {
            Assert.Contains("[BranchCode]", WarrantyClaimSearchSqlBuilder.Build(Query()).Sql);
        }

        [Fact]
        public void It_PagesWithOffsetAndFetch()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(skip: 100, take: 25));

            Assert.Contains("OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY", Normalize(command.Sql));
            Assert.Equal(100, ValueOf(command, "@Skip"));
            Assert.Equal(25, ValueOf(command, "@Take"));
        }

        /// <summary>
        /// Counted in its own statement over the same criteria, so a page starting past the end of
        /// the results still reports how many there are rather than none.
        /// </summary>
        [Fact]
        public void It_CountsTheWholeResultSetAlongsideThePage()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "291", skip: 100, take: 25));
            var sql = Normalize(command.Sql);

            Assert.Contains("SELECT COUNT(*) FROM [dbo].[WarrantyClaims] WHERE", sql);
            Assert.DoesNotContain("OVER()", sql);

            //Both statements narrow by the same criteria
            Assert.Equal(2, Regex.Matches(sql, @"\[ClaimIdentifier\] LIKE @Keyword").Count);
            Assert.Equal(2, Regex.Matches(sql, @"WHERE \[InstanceIdentifier\] = @InstanceIdentifier").Count);

            //...and only the page is ordered and offset
            Assert.Single(Regex.Matches(sql, "ORDER BY"));
            Assert.Single(Regex.Matches(sql, "OFFSET @Skip"));
        }

        /// <summary>
        /// A page of claims would otherwise carry a page of claim documents.
        /// </summary>
        [Fact]
        public void It_NeverReadsTheClaimDocument()
        {
            Assert.DoesNotContain("JsonData", WarrantyClaimSearchSqlBuilder.Build(Query()).Sql);
        }

        [Theory]
        [InlineData(-1, 50)]
        [InlineData(0, 0)]
        [InlineData(0, -10)]
        public void WhenThePageMakesNoSense_It_Throws(int skip, int take)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                WarrantyClaimSearchSqlBuilder.Build(Query(skip: skip, take: take)));
        }

        [Fact]
        public void WhenThereIsNoInstance_It_Throws()
        {
            var query = Query();
            query.InstanceIdentifier = null;

            Assert.Throws<ArgumentNullException>(() => WarrantyClaimSearchSqlBuilder.Build(query));
        }

        #endregion

        #region Keyword

        [Fact]
        public void WhenGivenAKeyword_It_MatchesEveryTextColumn()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "291"));
            var sql = Normalize(command.Sql);

            string[] expected =
            [
                "ClaimIdentifier", "RepairOrderIdentifier", "WarrantyRepairOrderIdentifier", "InvoiceIdentifier",
                "CustomerIdentifier", "CompanyName", "VehicleIdentifier", "CausalPartIdentifier", "Oem", "ClaimStatus"
            ];

            foreach (var column in expected)
            {
                Assert.Contains($"[{column}] LIKE @Keyword ESCAPE '\\'", sql);
            }

            //One clause, ORed together, so a keyword narrows nothing on its own. Twice over, because
            //the count and the page each carry the same criteria.
            Assert.Equal((expected.Length - 1) * 2, Regex.Matches(sql, @"OR \[\w+\] LIKE @Keyword").Count);
        }

        /// <summary>
        /// The requirement in its own terms: 291 has to find 2182912, 291234, 1291 and 291.
        /// </summary>
        [Fact]
        public void WhenGivenAKeyword_It_MatchesAnywhereInTheValue()
        {
            Assert.Equal("%291%", ValueOf(WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "291")), "@Keyword"));
        }

        [Fact]
        public void WhenGivenAKeywordAndTerms_It_Throws()
        {
            var terms = new[] { Term(WarrantyClaimField.ClaimStatus, WarrantyClaimSearchOperator.EqualTo, "Submitted") };

            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "291", terms: terms)));
        }

        [Fact]
        public void WhenTheKeywordIsBlank_It_SearchesEverything()
        {
            var sql = Normalize(WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "   ")).Sql);

            Assert.DoesNotContain("@Keyword", sql);
        }

        #endregion

        #region Terms

        [Fact]
        public void WhenMatchingContains_It_WrapsTheTermInWildcards()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.RepairOrderIdentifier, WarrantyClaimSearchOperator.Contains, "291")
            ]));

            Assert.Contains("AND [RepairOrderIdentifier] LIKE @t0 ESCAPE '\\'", Normalize(command.Sql));
            Assert.Equal("%291%", ValueOf(command, "@t0"));
        }

        [Fact]
        public void WhenMatchingBeginsWith_It_AnchorsTheTerm()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.RepairOrderIdentifier, WarrantyClaimSearchOperator.BeginsWith, "291")
            ]));

            Assert.Contains("AND [RepairOrderIdentifier] LIKE @t0 ESCAPE '\\'", Normalize(command.Sql));
            Assert.Equal("291%", ValueOf(command, "@t0"));
        }

        [Theory]
        [InlineData(WarrantyClaimSearchOperator.EqualTo, "AND [ClaimIdentifier] = @t0")]
        [InlineData(WarrantyClaimSearchOperator.GreaterThan, "AND [ClaimIdentifier] > @t0")]
        [InlineData(WarrantyClaimSearchOperator.LessThan, "AND [ClaimIdentifier] < @t0")]
        public void It_ComparesWithoutWildcards(WarrantyClaimSearchOperator @operator, string expected)
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.ClaimIdentifier, @operator, "C-1001")
            ]));

            Assert.Contains(expected, Normalize(command.Sql));
            Assert.Equal("C-1001", ValueOf(command, "@t0"));
        }

        /// <summary>
        /// A claim with no company name recorded is not a claim from ACME, so it belongs in the
        /// results. A plain &lt;&gt; would drop it.
        /// </summary>
        [Fact]
        public void WhenMatchingNotEqualTo_It_KeepsRowsWithNothingStored()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.CompanyName, WarrantyClaimSearchOperator.NotEqualTo, "ACME")
            ]));

            Assert.Contains("AND ([CompanyName] IS NULL OR [CompanyName] <> @t0)", Normalize(command.Sql));
        }

        [Fact]
        public void WhenGivenSeveralTerms_It_NarrowsByAllOfThem()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.RepairOrderIdentifier, WarrantyClaimSearchOperator.Contains, "291"),
                Term(WarrantyClaimField.ClaimTotal, WarrantyClaimSearchOperator.GreaterThan, 100m),
                Term(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.LessThan, new DateTime(2026, 3, 2))
            ]));

            var sql = Normalize(command.Sql);

            Assert.Contains("AND [RepairOrderIdentifier] LIKE @t0", sql);
            Assert.Contains("AND [ClaimTotal] > @t1", sql);
            Assert.Contains("AND [RepairOrderOpenedDate] < @t2", sql);
            Assert.Equal(100m, ValueOf(command, "@t1"));
            Assert.Equal(new DateTime(2026, 3, 2), ValueOf(command, "@t2"));
        }

        /// <summary>
        /// Two terms on one field are two predicates, not one overwriting the other: each takes its
        /// own parameter from its position, and they narrow the search together like any other pair.
        /// </summary>
        [Fact]
        public void WhenOneFieldIsGivenSeveralTerms_It_NarrowsByAllOfThem()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.ClaimTotal, WarrantyClaimSearchOperator.GreaterThan, 100m),
                Term(WarrantyClaimField.ClaimTotal, WarrantyClaimSearchOperator.LessThan, 500m)
            ]));

            var sql = Normalize(command.Sql);

            Assert.Contains("AND [ClaimTotal] > @t0", sql);
            Assert.Contains("AND [ClaimTotal] < @t1", sql);
            Assert.Equal(100m, ValueOf(command, "@t0"));
            Assert.Equal(500m, ValueOf(command, "@t1"));
        }

        [Fact]
        public void It_BindsEachTermToItsColumnType()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.CompanyName, WarrantyClaimSearchOperator.EqualTo, "ACME"),
                Term(WarrantyClaimField.ClaimTotal, WarrantyClaimSearchOperator.EqualTo, 100m),
                Term(WarrantyClaimField.CreatedOn, WarrantyClaimSearchOperator.GreaterThan, new DateTime(2026, 1, 1))
            ]));

            SqlParameter Parameter(string name) => command.Parameters.Single(p => p.ParameterName == name);

            //CompanyName is the nvarchar column; the varchar ones bind as varchar
            Assert.Equal(SqlDbType.NVarChar, Parameter("@t0").SqlDbType);
            Assert.Equal(SqlDbType.Decimal, Parameter("@t1").SqlDbType);
            Assert.Equal(SqlDbType.DateTime2, Parameter("@t2").SqlDbType);
            Assert.Equal(SqlDbType.VarChar, Parameter("@InstanceIdentifier").SqlDbType);
        }

        [Theory]
        [InlineData(WarrantyClaimSearchOperator.BeginsWith)]
        [InlineData(WarrantyClaimSearchOperator.Contains)]
        public void WhenMatchingTextAgainstANumber_It_Throws(WarrantyClaimSearchOperator @operator)
        {
            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.ClaimTotal, @operator, "100")
            ])));
        }

        [Fact]
        public void WhenAValueIsNotTheColumnType_It_Throws()
        {
            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.ClaimTotal, WarrantyClaimSearchOperator.EqualTo, "100")
            ])));

            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.CompanyName, WarrantyClaimSearchOperator.EqualTo, 100m)
            ])));

            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.EqualTo, "2026-03-02")
            ])));
        }

        #endregion

        #region Whole days

        /// <summary>
        /// A day is every moment in it. This is the comparison a date filter in the ui means, and the
        /// reason it can be written as one term: the two bounds are the builder's to add.
        /// </summary>
        [Fact]
        public void WhenMatchingADay_It_SpansTheWholeOfIt()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Day(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.EqualTo, SecondOfMarch)
            ]));

            Assert.Contains(
                "AND ([RepairOrderOpenedDate] >= @t0 AND [RepairOrderOpenedDate] < @t0End)",
                Normalize(command.Sql));
            Assert.Equal(SecondOfMarch, ValueOf(command, "@t0"));
            Assert.Equal(ThirdOfMarch, ValueOf(command, "@t0End"));
        }

        [Fact]
        public void WhenNotMatchingADay_It_KeepsEverythingOutsideIt()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Day(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.NotEqualTo, SecondOfMarch)
            ]));

            Assert.Contains(
                "AND ([RepairOrderOpenedDate] IS NULL OR [RepairOrderOpenedDate] < @t0 " +
                "OR [RepairOrderOpenedDate] >= @t0End)",
                Normalize(command.Sql));
        }

        /// <summary>
        /// After the second of March is the third onwards, not the second's own afternoon.
        /// </summary>
        [Fact]
        public void WhenMatchingAfterADay_It_StartsAtTheNextOne()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Day(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.GreaterThan, SecondOfMarch)
            ]));

            Assert.Contains("AND [RepairOrderOpenedDate] >= @t0End", Normalize(command.Sql));
            Assert.Equal(ThirdOfMarch, ValueOf(command, "@t0End"));
            Assert.False(HasParameter(command, "@t0"));
        }

        [Fact]
        public void WhenMatchingBeforeADay_It_StopsAtItsStart()
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Day(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.LessThan, SecondOfMarch)
            ]));

            Assert.Contains("AND [RepairOrderOpenedDate] < @t0", Normalize(command.Sql));
            Assert.Equal(SecondOfMarch, ValueOf(command, "@t0"));
            Assert.False(HasParameter(command, "@t0End"));
        }

        /// <summary>
        /// A caller who names a time means that moment, so nothing is widened for them.
        /// </summary>
        [Fact]
        public void WhenAValueNamesAMoment_It_ComparesThatMoment()
        {
            var moment = new DateTime(2026, 3, 2, 14, 30, 0);

            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.RepairOrderOpenedDate, WarrantyClaimSearchOperator.EqualTo, moment)
            ]));

            Assert.Contains("AND [RepairOrderOpenedDate] = @t0", Normalize(command.Sql));
            Assert.Equal(moment, ValueOf(command, "@t0"));
            Assert.False(HasParameter(command, "@t0End"));
        }

        /// <summary>
        /// The day of a claim identifier is not a thing, so a term flagged as one against a text
        /// column is compared as the text it holds.
        /// </summary>
        [Fact]
        public void WhenADayIsFlaggedOnATextColumn_It_ComparesTheValueItself()
        {
            var term = Term(WarrantyClaimField.ClaimIdentifier, WarrantyClaimSearchOperator.EqualTo, "C-1001");
            term.DateOnly = true;

            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms: [term]));

            Assert.Contains("AND [ClaimIdentifier] = @t0", Normalize(command.Sql));
            Assert.Equal("C-1001", ValueOf(command, "@t0"));
        }

        [Theory]
        [InlineData(WarrantyClaimSearchOperator.BeginsWith)]
        [InlineData(WarrantyClaimSearchOperator.Contains)]
        public void WhenMatchingTextAgainstADay_It_Throws(WarrantyClaimSearchOperator @operator)
        {
            Assert.Throws<ArgumentException>(() => WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Day(WarrantyClaimField.RepairOrderOpenedDate, @operator, SecondOfMarch)
            ])));
        }

        #endregion

        #region Escaping

        /// <summary>
        /// Searching for "50%" looks for those three characters. Left alone, the wildcard in the term
        /// would match anything starting with 50 and the search would quietly mean something else.
        /// </summary>
        [Theory]
        [InlineData("50%", "%50\\%%")]
        [InlineData("a_b", "%a\\_b%")]
        [InlineData("[abc]", "%\\[abc]%")]
        [InlineData("back\\slash", "%back\\\\slash%")]
        public void It_NeutralizesWildcardsInTheTerm(string term, string expected)
        {
            var command = WarrantyClaimSearchSqlBuilder.Build(Query(terms:
            [
                Term(WarrantyClaimField.ClaimIdentifier, WarrantyClaimSearchOperator.Contains, term)
            ]));

            Assert.Equal(expected, ValueOf(command, "@t0"));
        }

        [Fact]
        public void It_NeutralizesWildcardsInAKeyword()
        {
            Assert.Equal("%100\\%%", ValueOf(WarrantyClaimSearchSqlBuilder.Build(Query(keyword: "100%")), "@Keyword"));
        }

        #endregion

        #region Ordering

        [Fact]
        public void WhenNoOrderIsAskedFor_It_LeadsWithTheNewestWork()
        {
            Assert.Contains(
                "ORDER BY [RepairOrderOpenedDate] DESC, [Id] DESC",
                Normalize(WarrantyClaimSearchSqlBuilder.Build(Query()).Sql));
        }

        [Theory]
        [InlineData(true, "ORDER BY [CompanyName] DESC, [Id] DESC")]
        [InlineData(false, "ORDER BY [CompanyName] ASC, [Id] DESC")]
        public void WhenAnOrderIsAskedFor_It_SortsByIt(bool descending, string expected)
        {
            var query = Query();
            query.SortField = WarrantyClaimField.CompanyName;
            query.SortDescending = descending;

            Assert.Contains(expected, Normalize(WarrantyClaimSearchSqlBuilder.Build(query).Sql));
        }

        /// <summary>
        /// Sorting is the one place a caller's choice reaches the statement as a column name, so
        /// every field it can name has to arrive as a bracketed column and nothing else.
        /// </summary>
        [Fact]
        public void It_OnlyEverOrdersByAWhitelistedColumn()
        {
            foreach (var field in Enum.GetValues<WarrantyClaimField>())
            {
                var query = Query();
                query.SortField = field;

                Assert.Contains(
                    $"ORDER BY [{field}] ASC, [Id] DESC",
                    Normalize(WarrantyClaimSearchSqlBuilder.Build(query).Sql));
            }
        }

        #endregion
    }
}
