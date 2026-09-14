using System.Data;
using System.Text;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Microsoft.Data.SqlClient;

namespace Karmak.Integrations.Volvo.Common.Sql;

/// <summary>
/// The statement and parameters a <see cref="WarrantyClaimSearchQuery"/> turns into.
/// </summary>
public sealed record WarrantyClaimSearchCommand(string Sql, IReadOnlyList<SqlParameter> Parameters);

/// <summary>
/// Turns a search into sql. Kept apart from <see cref="WarrantyDataRepository"/> because it is the
/// only part of searching with anything to get wrong, and as a pure function it can be tested
/// without a database.
/// </summary>
/// <remarks>
/// Nothing a caller supplies is ever written into the statement. Column names come from
/// <see cref="Columns"/> and are reached through a <see cref="WarrantyClaimField"/>, and every value
/// travels as a parameter, so a search cannot express anything the whitelist does not allow.
/// </remarks>
public static class WarrantyClaimSearchSqlBuilder
{
    /// <summary>
    /// Escapes the wildcards in a LIKE pattern. Backslash rather than a bracket so the escaped
    /// pattern stays readable in a query plan.
    /// </summary>
    private const char EscapeCharacter = '\\';

    /// <summary>
    /// Text parameters are sized wider than any column they are compared against. A term longer
    /// than its column matches nothing, which is correct; a term silently truncated to the column's
    /// width would match something else, which is not.
    /// </summary>
    private const int TextParameterSize = 1024;

    private const string SelectColumns = @"
    [ClaimId],
    [InstanceIdentifier],
    [BranchIdentifier],
    [BranchCode],
    [CorrelationId],
    [IsDeleted],
    [CausalPartIdentifier],
    [ClaimIdentifier],
    [CompanyName],
    [ClaimTotal],
    [RepairOrderCompletedDate],
    [RepairOrderOpenedDate],
    [CustomerIdentifier],
    [InvoiceIdentifier],
    [Oem],
    [RepairOrderIdentifier],
    [WarrantyRepairOrderIdentifier],
    [ClaimStatus],
    [VehicleIdentifier],
    [CreatedOn],
    [UpdatedOn]";

    private sealed record ColumnDefinition(string Name, SqlDbType DbType)
    {
        public bool IsText => DbType is SqlDbType.VarChar or SqlDbType.NVarChar;

        public WarrantyClaimFieldType FieldType => DbType switch
        {
            SqlDbType.Decimal => WarrantyClaimFieldType.Number,
            SqlDbType.DateTime2 => WarrantyClaimFieldType.Date,
            _ => WarrantyClaimFieldType.Text
        };
    }

    /// <summary>
    /// The only column names this builder will ever emit, and the type each one binds as.
    /// </summary>
    private static readonly IReadOnlyDictionary<WarrantyClaimField, ColumnDefinition> Columns =
        new Dictionary<WarrantyClaimField, ColumnDefinition>
        {
            [WarrantyClaimField.ClaimId] = new("ClaimId", SqlDbType.VarChar),
            [WarrantyClaimField.CorrelationId] = new("CorrelationId", SqlDbType.VarChar),
            [WarrantyClaimField.BranchCode] = new("BranchCode", SqlDbType.VarChar),
            [WarrantyClaimField.CausalPartIdentifier] = new("CausalPartIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.ClaimIdentifier] = new("ClaimIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.CompanyName] = new("CompanyName", SqlDbType.NVarChar),
            [WarrantyClaimField.CustomerIdentifier] = new("CustomerIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.InvoiceIdentifier] = new("InvoiceIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.Oem] = new("Oem", SqlDbType.NVarChar),
            [WarrantyClaimField.RepairOrderIdentifier] = new("RepairOrderIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.WarrantyRepairOrderIdentifier] = new("WarrantyRepairOrderIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.ClaimStatus] = new("ClaimStatus", SqlDbType.NVarChar),
            [WarrantyClaimField.VehicleIdentifier] = new("VehicleIdentifier", SqlDbType.VarChar),
            [WarrantyClaimField.ClaimTotal] = new("ClaimTotal", SqlDbType.Decimal),
            [WarrantyClaimField.RepairOrderOpenedDate] = new("RepairOrderOpenedDate", SqlDbType.DateTime2),
            [WarrantyClaimField.RepairOrderCompletedDate] = new("RepairOrderCompletedDate", SqlDbType.DateTime2),
            [WarrantyClaimField.CreatedOn] = new("CreatedOn", SqlDbType.DateTime2),
            [WarrantyClaimField.UpdatedOn] = new("UpdatedOn", SqlDbType.DateTime2)
        };

    /// <summary>
    /// The columns a keyword sweeps. The identity columns are left out: a claim id or correlation
    /// id is something a caller pastes in whole, not something they recognize half of, and letting
    /// a keyword hit them turns short searches into noise. So is the branch code, which every row in
    /// a branch-scoped search shares and which therefore narrows nothing.
    /// </summary>
    private static readonly IReadOnlyList<WarrantyClaimField> KeywordFields =
    [
        WarrantyClaimField.ClaimIdentifier,
        WarrantyClaimField.RepairOrderIdentifier,
        WarrantyClaimField.WarrantyRepairOrderIdentifier,
        WarrantyClaimField.InvoiceIdentifier,
        WarrantyClaimField.CustomerIdentifier,
        WarrantyClaimField.CompanyName,
        WarrantyClaimField.VehicleIdentifier,
        WarrantyClaimField.CausalPartIdentifier,
        WarrantyClaimField.Oem,
        WarrantyClaimField.ClaimStatus
    ];

    /// <summary>
    /// Ordered by when the work came in, newest first, so an unsorted search opens on the claims a
    /// dealer is most likely looking for.
    /// </summary>
    private const WarrantyClaimField DefaultSortField = WarrantyClaimField.RepairOrderOpenedDate;

    /// <summary>
    /// The status a new-only search keeps. Written here rather than read from the warranty module's
    /// ClaimStatus, which sits above this layer and cannot be referenced from it.
    /// </summary>
    private const string NewClaimStatus = "New";

    /// <summary>
    /// What the field holds. Callers parse a term's value and decide whether an operator applies
    /// before building, and this is the same answer the builder itself works from.
    /// </summary>
    public static WarrantyClaimFieldType TypeOf(WarrantyClaimField field) => Columns[field].FieldType;

    public static WarrantyClaimSearchCommand Build(WarrantyClaimSearchQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.InstanceIdentifier);
        ArgumentOutOfRangeException.ThrowIfNegative(query.Skip);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(query.Take, 0);

        var terms = query.Terms ?? [];
        var keyword = string.IsNullOrWhiteSpace(query.Keyword) ? null : query.Keyword;

        if (keyword != null && terms.Count > 0)
        {
            throw new ArgumentException(
                "A search is either a keyword across every field or a set of field terms, not both.",
                nameof(query));
        }

        var parameters = new List<SqlParameter>
        {
            Parameter("@InstanceIdentifier", SqlDbType.VarChar, TextParameterSize, query.InstanceIdentifier),
            Parameter("@BranchIdentifier", SqlDbType.VarChar, TextParameterSize, query.BranchIdentifier),
            new("@IncludeDeleted", SqlDbType.Bit) { Value = query.IncludeDeleted },
            new("@NewOnly", SqlDbType.Bit) { Value = query.NewOnly },
            Parameter("@NewClaimStatus", SqlDbType.NVarChar, TextParameterSize, NewClaimStatus),
            new("@Skip", SqlDbType.Int) { Value = query.Skip },
            new("@Take", SqlDbType.Int) { Value = query.Take }
        };

        //Every optional predicate is written as "@Parameter IS NULL OR ...", so one plan shape serves
        //every combination, and RECOMPILE below keeps the optimizer from caching the first one it saw.
        var where = new StringBuilder()
            .AppendLine("WHERE [InstanceIdentifier] = @InstanceIdentifier")
            .AppendLine("  AND (@BranchIdentifier IS NULL OR [BranchIdentifier] = @BranchIdentifier)")
            .AppendLine("  AND (@IncludeDeleted = 1 OR [IsDeleted] = 0)")
            .AppendLine("  AND (@NewOnly = 0 OR [ClaimStatus] = @NewClaimStatus)");

        if (keyword != null)
        {
            AppendKeyword(where, parameters, keyword);
        }

        for (var index = 0; index < terms.Count; index++)
        {
            AppendTerm(where, parameters, terms[index], index);
        }

        //The count is its own statement rather than a COUNT(*) OVER() riding along with the page. The
        //window would read as zero for a page that starts past the end of the results, which is what
        //a caller paging through a set that shrank under them would see, and it is not true.
        var sql = new StringBuilder()
            .AppendLine("SELECT COUNT(*)")
            .AppendLine("FROM [dbo].[WarrantyClaims]")
            .Append(where)
            .AppendLine("OPTION (RECOMPILE);")
            .AppendLine()
            .Append("SELECT")
            .AppendLine(SelectColumns)
            .AppendLine("FROM [dbo].[WarrantyClaims]")
            .Append(where)
            .AppendLine(BuildOrderBy(query))
            .AppendLine("OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY")
            .Append("OPTION (RECOMPILE);");

        return new WarrantyClaimSearchCommand(sql.ToString(), parameters);
    }

    private static void AppendKeyword(StringBuilder sql, List<SqlParameter> parameters, string keyword)
    {
        var matches = KeywordFields
            .Select(field => $"[{Columns[field].Name}] LIKE @Keyword ESCAPE '{EscapeCharacter}'");

        sql.Append("  AND (")
            .Append(string.Join($"{Environment.NewLine}       OR ", matches))
            .AppendLine(")");

        //One nvarchar parameter serves both the varchar and the nvarchar columns. A keyword is a
        //"contains" match either way, so the comparison is a scan whichever type it widens to.
        parameters.Add(Parameter("@Keyword", SqlDbType.NVarChar, TextParameterSize, Contains(keyword)));
    }

    private static void AppendTerm(
        StringBuilder sql,
        List<SqlParameter> parameters,
        WarrantyClaimSearchTerm term,
        int index)
    {
        ArgumentNullException.ThrowIfNull(term);

        var column = Columns[term.Field];
        var name = $"@t{index}";

        EnsureOperatorApplies(column, term.Operator);

        //A term naming a day is compared against the whole of it, which takes both of the day's
        //bounds rather than the single value every other comparison is written against.
        if (term.DateOnly && column.FieldType == WarrantyClaimFieldType.Date && term.Value is DateTime day)
        {
            AppendWholeDayTerm(sql, parameters, column, term.Operator, day.Date, name);
            return;
        }

        sql.AppendLine($"  AND {BuildComparison(column, term.Operator, name)}");
        parameters.Add(BuildParameter(column, term, name));
    }

    /// <summary>
    /// A day, compared as the whole of itself. Equal to the day is every moment in it; not equal to
    /// it is every moment outside it; after the day starts once it has ended, and before the day ends
    /// where it began. This is what anyone filtering on a date means, and it is why "after the second
    /// of March" does not hand back the afternoon of the second.
    /// </summary>
    private static void AppendWholeDayTerm(
        StringBuilder sql,
        List<SqlParameter> parameters,
        ColumnDefinition column,
        WarrantyClaimSearchOperator @operator,
        DateTime day,
        string name)
    {
        var start = name;
        var end = $"{name}End";

        switch (@operator)
        {
            case WarrantyClaimSearchOperator.EqualTo:
                sql.AppendLine($"  AND ([{column.Name}] >= {start} AND [{column.Name}] < {end})");
                parameters.Add(DayBound(start, day));
                parameters.Add(DayBound(end, day.AddDays(1)));
                break;

            //As with any other inequality, a row with nothing stored is not that day, so it stays in.
            case WarrantyClaimSearchOperator.NotEqualTo:
                sql.AppendLine(
                    $"  AND ([{column.Name}] IS NULL OR [{column.Name}] < {start} OR [{column.Name}] >= {end})");
                parameters.Add(DayBound(start, day));
                parameters.Add(DayBound(end, day.AddDays(1)));
                break;

            case WarrantyClaimSearchOperator.GreaterThan:
                sql.AppendLine($"  AND [{column.Name}] >= {end}");
                parameters.Add(DayBound(end, day.AddDays(1)));
                break;

            case WarrantyClaimSearchOperator.LessThan:
                sql.AppendLine($"  AND [{column.Name}] < {start}");
                parameters.Add(DayBound(start, day));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(@operator), @operator, "Unknown search operator.");
        }
    }

    private static SqlParameter DayBound(string name, DateTime value) =>
        new(name, SqlDbType.DateTime2) { Value = value };

    private static void EnsureOperatorApplies(ColumnDefinition column, WarrantyClaimSearchOperator @operator)
    {
        if (@operator is WarrantyClaimSearchOperator.BeginsWith or WarrantyClaimSearchOperator.Contains
            && !column.IsText)
        {
            throw new ArgumentException(
                $"'{@operator}' matches text and [{column.Name}] does not hold text.", nameof(@operator));
        }
    }

    private static string BuildComparison(ColumnDefinition column, WarrantyClaimSearchOperator @operator, string name)
    {
        return @operator switch
        {
            WarrantyClaimSearchOperator.BeginsWith or WarrantyClaimSearchOperator.Contains =>
                $"[{column.Name}] LIKE {name} ESCAPE '{EscapeCharacter}'",
            WarrantyClaimSearchOperator.EqualTo => $"[{column.Name}] = {name}",
            //A row with no value stored is not equal to the term, so it belongs in the result. This
            //is the same reading of an inequality against a missing property that the cosmos queries
            //had, and that WarrantyDataRepository's correlation id exclusion still keeps.
            WarrantyClaimSearchOperator.NotEqualTo => $"([{column.Name}] IS NULL OR [{column.Name}] <> {name})",
            WarrantyClaimSearchOperator.GreaterThan => $"[{column.Name}] > {name}",
            WarrantyClaimSearchOperator.LessThan => $"[{column.Name}] < {name}",
            _ => throw new ArgumentOutOfRangeException(nameof(@operator), @operator, "Unknown search operator.")
        };
    }

    private static SqlParameter BuildParameter(ColumnDefinition column, WarrantyClaimSearchTerm term, string name)
    {
        if (column.IsText)
        {
            if (term.Value is not string text)
            {
                throw new ArgumentException($"[{column.Name}] holds text and was given {Describe(term.Value)}.", nameof(term));
            }

            var value = term.Operator switch
            {
                WarrantyClaimSearchOperator.BeginsWith => BeginsWith(text),
                WarrantyClaimSearchOperator.Contains => Contains(text),
                _ => text
            };

            return Parameter(name, column.DbType, TextParameterSize, value);
        }

        if (column.DbType == SqlDbType.Decimal)
        {
            if (term.Value is not decimal number)
            {
                throw new ArgumentException($"[{column.Name}] holds a number and was given {Describe(term.Value)}.", nameof(term));
            }

            return new SqlParameter(name, SqlDbType.Decimal) { Precision = 19, Scale = 4, Value = number };
        }

        if (term.Value is not DateTime date)
        {
            throw new ArgumentException($"[{column.Name}] holds a date and was given {Describe(term.Value)}.", nameof(term));
        }

        return new SqlParameter(name, SqlDbType.DateTime2) { Value = date };
    }

    private static string BuildOrderBy(WarrantyClaimSearchQuery query)
    {
        //With no sort asked for, the newest work leads; naming a field means the caller decides the
        //direction. [Id] breaks ties so a row cannot appear on two pages or on none.
        var field = query.SortField ?? DefaultSortField;
        var descending = !query.SortField.HasValue || query.SortDescending;

        return $"ORDER BY [{Columns[field].Name}] {(descending ? "DESC" : "ASC")}, [Id] DESC";
    }

    private static string BeginsWith(string term) => $"{Escape(term)}%";

    private static string Contains(string term) => $"%{Escape(term)}%";

    /// <summary>
    /// Neutralizes the LIKE wildcards in a term, so searching for "50%" looks for the two characters
    /// rather than for anything starting with 50. The escape character goes first or it would escape
    /// the escapes added after it.
    /// </summary>
    private static string Escape(string term) => term
        .Replace($"{EscapeCharacter}", $"{EscapeCharacter}{EscapeCharacter}")
        .Replace("%", $"{EscapeCharacter}%")
        .Replace("_", $"{EscapeCharacter}_")
        .Replace("[", $"{EscapeCharacter}[");

    private static SqlParameter Parameter(string name, SqlDbType type, int size, string value)
    {
        return new SqlParameter(name, type, size)
        {
            Value = string.IsNullOrEmpty(value) ? DBNull.Value : value
        };
    }

    private static string Describe(object value) => value == null ? "nothing" : $"a {value.GetType().Name}";
}
