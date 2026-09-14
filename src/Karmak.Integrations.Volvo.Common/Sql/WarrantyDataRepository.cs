using System.Data;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Microsoft.Data.SqlClient;

namespace Karmak.Integrations.Volvo.Common.Sql;

public class WarrantyDataRepository : IWarrantyDataRepository
{
    // JsonData is a native JSON column; cast it so the reader always hands back a string.
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
    [UpdatedOn],
    CAST([JsonData] AS NVARCHAR(MAX)) AS [JsonData]";

    // MERGE under HOLDLOCK so two writers racing on the same claim serialize against the
    // unique index rather than both falling through to an insert. CreatedOn is only set by
    // the insert branch, so it keeps recording when the claim first reached sql.
    private const string UpsertSql = @"
MERGE [dbo].[WarrantyClaims] WITH (HOLDLOCK) AS target
USING (SELECT @InstanceIdentifier AS [InstanceIdentifier], @ClaimId AS [ClaimId]) AS source
    ON target.[InstanceIdentifier] = source.[InstanceIdentifier]
   AND target.[ClaimId] = source.[ClaimId]
WHEN MATCHED THEN UPDATE SET
    [BranchIdentifier]              = @BranchIdentifier,
    [BranchCode]                    = @BranchCode,
    [CorrelationId]                 = @CorrelationId,
    [IsDeleted]                     = @IsDeleted,
    [CausalPartIdentifier]          = @CausalPartIdentifier,
    [ClaimIdentifier]               = @ClaimIdentifier,
    [CompanyName]                   = @CompanyName,
    [ClaimTotal]                    = @ClaimTotal,
    [RepairOrderCompletedDate]      = @RepairOrderCompletedDate,
    [RepairOrderOpenedDate]         = @RepairOrderOpenedDate,
    [CustomerIdentifier]            = @CustomerIdentifier,
    [InvoiceIdentifier]             = @InvoiceIdentifier,
    [Oem]                           = @Oem,
    [RepairOrderIdentifier]         = @RepairOrderIdentifier,
    [WarrantyRepairOrderIdentifier] = @WarrantyRepairOrderIdentifier,
    [ClaimStatus]                   = @ClaimStatus,
    [VehicleIdentifier]             = @VehicleIdentifier,
    [UpdatedOn]                     = @Now,
    [JsonData]                      = @JsonData
WHEN NOT MATCHED THEN INSERT
(
    [ClaimId], [InstanceIdentifier], [BranchIdentifier], [BranchCode], [CorrelationId], [IsDeleted],
    [CausalPartIdentifier], [ClaimIdentifier], [CompanyName], [ClaimTotal],
    [RepairOrderCompletedDate], [RepairOrderOpenedDate], [CustomerIdentifier],
    [InvoiceIdentifier], [Oem], [RepairOrderIdentifier], [WarrantyRepairOrderIdentifier],
    [ClaimStatus], [VehicleIdentifier], [CreatedOn], [UpdatedOn], [JsonData]
)
VALUES
(
    @ClaimId, @InstanceIdentifier, @BranchIdentifier, @BranchCode, @CorrelationId, @IsDeleted,
    @CausalPartIdentifier, @ClaimIdentifier, @CompanyName, @ClaimTotal,
    @RepairOrderCompletedDate, @RepairOrderOpenedDate, @CustomerIdentifier,
    @InvoiceIdentifier, @Oem, @RepairOrderIdentifier, @WarrantyRepairOrderIdentifier,
    @ClaimStatus, @VehicleIdentifier, @Now, @Now, @JsonData
);";

    private const string FindSql = @"
SELECT" + SelectColumns + @"
FROM [dbo].[WarrantyClaims]
WHERE [InstanceIdentifier] = @InstanceIdentifier
  AND [ClaimId] = @ClaimId;";

    // Every optional predicate is written as "@Parameter IS NULL OR ...", so one plan shape
    // serves every combination. RECOMPILE keeps the optimizer from caching a plan built for
    // whichever combination happened to run first.
    private const string QuerySql = @"
SELECT" + SelectColumns + @"
FROM [dbo].[WarrantyClaims]
WHERE [InstanceIdentifier] = @InstanceIdentifier
  AND (@BranchIdentifier IS NULL OR [BranchIdentifier] = @BranchIdentifier)
  AND (@IsDeleted IS NULL OR [IsDeleted] = @IsDeleted)
  AND (@CorrelationId IS NULL
       OR (@ExcludeCorrelationId = 0 AND [CorrelationId] = @CorrelationId)
       OR (@ExcludeCorrelationId = 1 AND ([CorrelationId] IS NULL OR [CorrelationId] <> @CorrelationId)))
  AND (@CausalPartIdentifier IS NULL OR [CausalPartIdentifier] = @CausalPartIdentifier)
  AND (@ClaimIdentifier IS NULL OR [ClaimIdentifier] = @ClaimIdentifier)
  AND (@CompanyName IS NULL OR [CompanyName] = @CompanyName)
  AND (@CustomerIdentifier IS NULL OR [CustomerIdentifier] = @CustomerIdentifier)
  AND (@InvoiceIdentifier IS NULL OR [InvoiceIdentifier] = @InvoiceIdentifier)
  AND (@Oem IS NULL OR [Oem] = @Oem)
  AND (@RepairOrderIdentifier IS NULL OR [RepairOrderIdentifier] = @RepairOrderIdentifier)
  AND (@WarrantyRepairOrderIdentifier IS NULL OR [WarrantyRepairOrderIdentifier] = @WarrantyRepairOrderIdentifier)
  AND (@ClaimStatus IS NULL OR [ClaimStatus] = @ClaimStatus)
  AND (@VehicleIdentifier IS NULL OR [VehicleIdentifier] = @VehicleIdentifier)
  AND (@ClaimTotalFrom IS NULL OR [ClaimTotal] >= @ClaimTotalFrom)
  AND (@ClaimTotalTo IS NULL OR [ClaimTotal] <= @ClaimTotalTo)
  AND (@RepairOrderOpenedFrom IS NULL OR [RepairOrderOpenedDate] >= @RepairOrderOpenedFrom)
  AND (@RepairOrderOpenedTo IS NULL OR [RepairOrderOpenedDate] <= @RepairOrderOpenedTo)
  AND (@RepairOrderCompletedFrom IS NULL OR [RepairOrderCompletedDate] >= @RepairOrderCompletedFrom)
  AND (@RepairOrderCompletedTo IS NULL OR [RepairOrderCompletedDate] <= @RepairOrderCompletedTo)
OPTION (RECOMPILE);";

    private const string CountSql = @"
SELECT COUNT_BIG(1)
FROM [dbo].[WarrantyClaims]
WHERE [InstanceIdentifier] = @InstanceIdentifier;";

    private const int IdentifierLength = 128;
    private const int NameLength = 256;

    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly TimeProvider _timeProvider;

    public WarrantyDataRepository(ISqlConnectionFactory connectionFactory, TimeProvider timeProvider)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task UpsertAsync(WarrantyClaimEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.ClaimId);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.InstanceIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(entity.JsonData);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(UpsertSql, connection);

        AddVarChar(command, "@ClaimId", entity.ClaimId, IdentifierLength);
        AddVarChar(command, "@InstanceIdentifier", entity.InstanceIdentifier, IdentifierLength);
        AddVarChar(command, "@BranchIdentifier", entity.BranchIdentifier, IdentifierLength);
        AddVarChar(command, "@BranchCode", entity.BranchCode, IdentifierLength);
        AddVarChar(command, "@CorrelationId", entity.CorrelationId, IdentifierLength);
        command.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit) { Value = entity.IsDeleted });

        AddVarChar(command, "@CausalPartIdentifier", entity.CausalPartIdentifier, IdentifierLength);
        AddVarChar(command, "@ClaimIdentifier", entity.ClaimIdentifier, IdentifierLength);
        AddNVarChar(command, "@CompanyName", entity.CompanyName, NameLength);
        AddDecimal(command, "@ClaimTotal", entity.ClaimTotal);
        AddDateTime(command, "@RepairOrderCompletedDate", entity.RepairOrderCompletedDate);
        AddDateTime(command, "@RepairOrderOpenedDate", entity.RepairOrderOpenedDate);
        AddVarChar(command, "@CustomerIdentifier", entity.CustomerIdentifier, IdentifierLength);
        AddVarChar(command, "@InvoiceIdentifier", entity.InvoiceIdentifier, IdentifierLength);
        AddNVarChar(command, "@Oem", entity.Oem, NameLength);
        AddVarChar(command, "@RepairOrderIdentifier", entity.RepairOrderIdentifier, IdentifierLength);
        AddVarChar(command, "@WarrantyRepairOrderIdentifier", entity.WarrantyRepairOrderIdentifier, IdentifierLength);
        AddNVarChar(command, "@ClaimStatus", entity.ClaimStatus, NameLength);
        AddVarChar(command, "@VehicleIdentifier", entity.VehicleIdentifier, IdentifierLength);

        command.Parameters.Add(new SqlParameter("@Now", SqlDbType.DateTime2) { Value = _timeProvider.GetUtcNow().UtcDateTime });
        command.Parameters.Add(new SqlParameter("@JsonData", SqlDbType.Json) { Value = entity.JsonData });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<WarrantyClaimEntity> FindAsync(
        string instanceIdentifier,
        string claimId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(claimId);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(FindSql, connection);

        AddVarChar(command, "@InstanceIdentifier", instanceIdentifier, IdentifierLength);
        AddVarChar(command, "@ClaimId", claimId, IdentifierLength);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken) ? ReadEntity(reader) : null;
    }

    public async Task<List<WarrantyClaimEntity>> QueryAsync(
        WarrantyClaimQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.InstanceIdentifier);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(QuerySql, connection);

        AddVarChar(command, "@InstanceIdentifier", query.InstanceIdentifier, IdentifierLength);
        AddVarChar(command, "@BranchIdentifier", query.BranchIdentifier, IdentifierLength);
        AddVarChar(command, "@CorrelationId", query.CorrelationId, IdentifierLength);
        command.Parameters.Add(new SqlParameter("@ExcludeCorrelationId", SqlDbType.Bit) { Value = query.ExcludeCorrelationId });
        command.Parameters.Add(new SqlParameter("@IsDeleted", SqlDbType.Bit)
        {
            Value = (object)query.IsDeleted ?? DBNull.Value
        });

        AddVarChar(command, "@CausalPartIdentifier", query.CausalPartIdentifier, IdentifierLength);
        AddVarChar(command, "@ClaimIdentifier", query.ClaimIdentifier, IdentifierLength);
        AddNVarChar(command, "@CompanyName", query.CompanyName, NameLength);
        AddVarChar(command, "@CustomerIdentifier", query.CustomerIdentifier, IdentifierLength);
        AddVarChar(command, "@InvoiceIdentifier", query.InvoiceIdentifier, IdentifierLength);
        AddNVarChar(command, "@Oem", query.Oem, NameLength);
        AddVarChar(command, "@RepairOrderIdentifier", query.RepairOrderIdentifier, IdentifierLength);
        AddVarChar(command, "@WarrantyRepairOrderIdentifier", query.WarrantyRepairOrderIdentifier, IdentifierLength);
        AddNVarChar(command, "@ClaimStatus", query.ClaimStatus, NameLength);
        AddVarChar(command, "@VehicleIdentifier", query.VehicleIdentifier, IdentifierLength);

        AddDecimal(command, "@ClaimTotalFrom", query.ClaimTotalFrom);
        AddDecimal(command, "@ClaimTotalTo", query.ClaimTotalTo);
        AddDateTime(command, "@RepairOrderOpenedFrom", query.RepairOrderOpenedFrom);
        AddDateTime(command, "@RepairOrderOpenedTo", query.RepairOrderOpenedTo);
        AddDateTime(command, "@RepairOrderCompletedFrom", query.RepairOrderCompletedFrom);
        AddDateTime(command, "@RepairOrderCompletedTo", query.RepairOrderCompletedTo);

        var entities = new List<WarrantyClaimEntity>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entities.Add(ReadEntity(reader));
        }

        return entities;
    }

    public async Task<WarrantyClaimSearchPage> SearchAsync(
        WarrantyClaimSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        //Unlike the fixed lookups above, a search's shape follows its criteria, so the statement is
        //built rather than written out. The builder is where that happens and where it is tested.
        var search = WarrantyClaimSearchSqlBuilder.Build(query);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(search.Sql, connection);

        command.Parameters.AddRange([.. search.Parameters]);

        var page = new WarrantyClaimSearchPage();

        //Two statements in the one command: the size of the whole result set, then the page of it.
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            page.TotalCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            page.Items.Add(ReadSummary(reader));
        }

        return page;
    }

    public async Task<int> CountAsync(string instanceIdentifier, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceIdentifier);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = CreateCommand(CountSql, connection);

        AddVarChar(command, "@InstanceIdentifier", instanceIdentifier, IdentifierLength);

        var count = await command.ExecuteScalarAsync(cancellationToken);

        return Convert.ToInt32(count);
    }

    private SqlCommand CreateCommand(string commandText, SqlConnection connection)
    {
        return new SqlCommand(commandText, connection)
        {
            CommandTimeout = _connectionFactory.CommandTimeoutSeconds
        };
    }

    private static void AddVarChar(SqlCommand command, string name, string value, int size)
    {
        command.Parameters.Add(new SqlParameter(name, SqlDbType.VarChar, size)
        {
            Value = string.IsNullOrEmpty(value) ? DBNull.Value : value
        });
    }

    private static void AddNVarChar(SqlCommand command, string name, string value, int size)
    {
        command.Parameters.Add(new SqlParameter(name, SqlDbType.NVarChar, size)
        {
            Value = string.IsNullOrEmpty(value) ? DBNull.Value : value
        });
    }

    private static void AddDecimal(SqlCommand command, string name, decimal? value)
    {
        command.Parameters.Add(new SqlParameter(name, SqlDbType.Decimal)
        {
            Precision = 19,
            Scale = 4,
            Value = (object)value ?? DBNull.Value
        });
    }

    private static void AddDateTime(SqlCommand command, string name, DateTime? value)
    {
        command.Parameters.Add(new SqlParameter(name, SqlDbType.DateTime2)
        {
            Value = (object)value ?? DBNull.Value
        });
    }

    private static WarrantyClaimEntity ReadEntity(SqlDataReader reader)
    {
        return new WarrantyClaimEntity
        {
            ClaimId = GetString(reader, nameof(WarrantyClaimEntity.ClaimId)),
            InstanceIdentifier = GetString(reader, nameof(WarrantyClaimEntity.InstanceIdentifier)),
            BranchIdentifier = GetString(reader, nameof(WarrantyClaimEntity.BranchIdentifier)),
            BranchCode = GetString(reader, nameof(WarrantyClaimEntity.BranchCode)),
            CorrelationId = GetString(reader, nameof(WarrantyClaimEntity.CorrelationId)),
            IsDeleted = reader[nameof(WarrantyClaimEntity.IsDeleted)] is bool isDeleted && isDeleted,
            CausalPartIdentifier = GetString(reader, nameof(WarrantyClaimEntity.CausalPartIdentifier)),
            ClaimIdentifier = GetString(reader, nameof(WarrantyClaimEntity.ClaimIdentifier)),
            CompanyName = GetString(reader, nameof(WarrantyClaimEntity.CompanyName)),
            ClaimTotal = GetDecimal(reader, nameof(WarrantyClaimEntity.ClaimTotal)),
            RepairOrderCompletedDate = GetDateTime(reader, nameof(WarrantyClaimEntity.RepairOrderCompletedDate)),
            RepairOrderOpenedDate = GetDateTime(reader, nameof(WarrantyClaimEntity.RepairOrderOpenedDate)),
            CustomerIdentifier = GetString(reader, nameof(WarrantyClaimEntity.CustomerIdentifier)),
            InvoiceIdentifier = GetString(reader, nameof(WarrantyClaimEntity.InvoiceIdentifier)),
            Oem = GetString(reader, nameof(WarrantyClaimEntity.Oem)),
            RepairOrderIdentifier = GetString(reader, nameof(WarrantyClaimEntity.RepairOrderIdentifier)),
            WarrantyRepairOrderIdentifier = GetString(reader, nameof(WarrantyClaimEntity.WarrantyRepairOrderIdentifier)),
            ClaimStatus = GetString(reader, nameof(WarrantyClaimEntity.ClaimStatus)),
            VehicleIdentifier = GetString(reader, nameof(WarrantyClaimEntity.VehicleIdentifier)),
            CreatedOn = GetDateTime(reader, nameof(WarrantyClaimEntity.CreatedOn)) ?? default,
            UpdatedOn = GetDateTime(reader, nameof(WarrantyClaimEntity.UpdatedOn)) ?? default,
            JsonData = GetString(reader, nameof(WarrantyClaimEntity.JsonData))
        };
    }

    private static WarrantyClaimSummary ReadSummary(SqlDataReader reader)
    {
        return new WarrantyClaimSummary
        {
            ClaimId = GetString(reader, nameof(WarrantyClaimSummary.ClaimId)),
            InstanceIdentifier = GetString(reader, nameof(WarrantyClaimSummary.InstanceIdentifier)),
            BranchIdentifier = GetString(reader, nameof(WarrantyClaimSummary.BranchIdentifier)),
            BranchCode = GetString(reader, nameof(WarrantyClaimSummary.BranchCode)),
            CorrelationId = GetString(reader, nameof(WarrantyClaimSummary.CorrelationId)),
            IsDeleted = reader[nameof(WarrantyClaimSummary.IsDeleted)] is bool isDeleted && isDeleted,
            CausalPartIdentifier = GetString(reader, nameof(WarrantyClaimSummary.CausalPartIdentifier)),
            ClaimIdentifier = GetString(reader, nameof(WarrantyClaimSummary.ClaimIdentifier)),
            CompanyName = GetString(reader, nameof(WarrantyClaimSummary.CompanyName)),
            ClaimTotal = GetDecimal(reader, nameof(WarrantyClaimSummary.ClaimTotal)),
            RepairOrderCompletedDate = GetDateTime(reader, nameof(WarrantyClaimSummary.RepairOrderCompletedDate)),
            RepairOrderOpenedDate = GetDateTime(reader, nameof(WarrantyClaimSummary.RepairOrderOpenedDate)),
            CustomerIdentifier = GetString(reader, nameof(WarrantyClaimSummary.CustomerIdentifier)),
            InvoiceIdentifier = GetString(reader, nameof(WarrantyClaimSummary.InvoiceIdentifier)),
            Oem = GetString(reader, nameof(WarrantyClaimSummary.Oem)),
            RepairOrderIdentifier = GetString(reader, nameof(WarrantyClaimSummary.RepairOrderIdentifier)),
            WarrantyRepairOrderIdentifier = GetString(reader, nameof(WarrantyClaimSummary.WarrantyRepairOrderIdentifier)),
            ClaimStatus = GetString(reader, nameof(WarrantyClaimSummary.ClaimStatus)),
            VehicleIdentifier = GetString(reader, nameof(WarrantyClaimSummary.VehicleIdentifier)),
            CreatedOn = GetDateTime(reader, nameof(WarrantyClaimSummary.CreatedOn)) ?? default,
            UpdatedOn = GetDateTime(reader, nameof(WarrantyClaimSummary.UpdatedOn)) ?? default
        };
    }

    //DBNull never matches the pattern, so a null column reads back as the type's null
    private static string GetString(SqlDataReader reader, string column) =>
        reader[column] is string value ? value : null;

    private static decimal? GetDecimal(SqlDataReader reader, string column) =>
        reader[column] is decimal value ? value : null;

    private static DateTime? GetDateTime(SqlDataReader reader, string column) =>
        reader[column] is DateTime value ? value : null;
}
