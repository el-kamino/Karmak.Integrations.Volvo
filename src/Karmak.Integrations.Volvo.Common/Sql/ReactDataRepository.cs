using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Microsoft.Data.SqlClient;

namespace Karmak.Integrations.Volvo.Common.Sql;

public class ReactDataRepository : IReactDataRepository
{
    private const string InsertSql = @"
INSERT INTO [dbo].[ReactDataEntities]
    ([EntityType], [EntityId], [CreatedOn], [KAN], [PACode], [JsonData])
VALUES
    (@EntityType, @EntityId, @CreatedOn, @KAN, @PACode, @JsonData);";

    // JsonData is a native JSON column; cast it so the reader always hands back a string.
    // @EntityIds is a JSON array so the id list stays a single parameter instead of a
    // dynamically built IN list.
    private const string QuerySql = @"
SELECT CAST([JsonData] AS NVARCHAR(MAX))
FROM [dbo].[ReactDataEntities]
WHERE [PACode] = @PACode
  AND [EntityType] = @EntityType
  AND (@HasEntityIds = 0 OR [EntityId] IN (SELECT [value] FROM OPENJSON(@EntityIds)))
  AND (@WindowStart IS NULL OR [CreatedOn] > @WindowStart)
  AND (@WindowEnd IS NULL OR [CreatedOn] < @WindowEnd);";

    // Deleted in batches rather than one statement so a large purge cannot hold a long
    // transaction or escalate to a table lock while dispatchers are still inserting.
    private const string DeleteSql = @"
DELETE TOP (@BatchSize) FROM [dbo].[ReactDataEntities]
WHERE [EntityType] = @EntityType
  AND [CreatedOn] < @Cutoff;";

    private static readonly JsonSerializerOptions SerializerOptions = new();

    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly TimeProvider _timeProvider;

    public ReactDataRepository(ISqlConnectionFactory connectionFactory, TimeProvider timeProvider)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task CreateAsync<T>(ReactDataEntity<T> entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(InsertSql, connection)
        {
            CommandTimeout = _connectionFactory.CommandTimeoutSeconds
        };

        string json = JsonSerializer.Serialize(entity.Entity, SerializerOptions);

        command.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.VarChar, 32) { Value = entity.EntityType });
        command.Parameters.Add(new SqlParameter("@EntityId", SqlDbType.VarChar, 128) { Value = entity.EntityId ?? (object)DBNull.Value });
        command.Parameters.Add(new SqlParameter("@CreatedOn", SqlDbType.DateTime2) { Value = _timeProvider.GetUtcNow().DateTime });
        command.Parameters.Add(new SqlParameter("@KAN", SqlDbType.VarChar, 32) { Value = entity.KAN });
        command.Parameters.Add(new SqlParameter("@PACode", SqlDbType.VarChar, 32) { Value = entity.PACode });
        command.Parameters.Add(new SqlParameter("@JsonData", SqlDbType.Json) { Value = json });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<List<T>> QueryAsync<T>(
        string paCode,
        string entityType,
        List<string> entitityIds,
        DateTime? windowStart = null,
        DateTime? windowEnd = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);

        bool hasEntityIds = entitityIds is { Count: > 0 };

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(QuerySql, connection)
        {
            CommandTimeout = _connectionFactory.CommandTimeoutSeconds
        };

        command.Parameters.Add(new SqlParameter("@PACode", SqlDbType.VarChar, 32) { Value = paCode });
        command.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.VarChar, 32) { Value = entityType });
        command.Parameters.Add(new SqlParameter("@HasEntityIds", SqlDbType.Bit) { Value = hasEntityIds });
        command.Parameters.Add(new SqlParameter("@EntityIds", SqlDbType.NVarChar, -1)
        {
            Value = hasEntityIds ? JsonSerializer.Serialize(entitityIds, SerializerOptions) : "[]"
        });
        command.Parameters.Add(new SqlParameter("@WindowStart", SqlDbType.DateTime2) { Value = (object)windowStart ?? DBNull.Value });
        command.Parameters.Add(new SqlParameter("@WindowEnd", SqlDbType.DateTime2) { Value = (object)windowEnd ?? DBNull.Value });

        var entities = new List<T>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (await reader.IsDBNullAsync(0, cancellationToken))
            {
                continue;
            }

            T entity = JsonSerializer.Deserialize<T>(reader.GetString(0), SerializerOptions);
            if (entity is not null)
            {
                entities.Add(entity);
            }
        }

        return entities;
    }

    public async Task<int> DeleteOlderThanAsync(
        string entityType,
        DateTime cutoffUtc,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        int deleted = 0;
        int batchDeleted;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var command = new SqlCommand(DeleteSql, connection)
            {
                CommandTimeout = _connectionFactory.CommandTimeoutSeconds
            };

            command.Parameters.Add(new SqlParameter("@BatchSize", SqlDbType.Int) { Value = batchSize });
            command.Parameters.Add(new SqlParameter("@EntityType", SqlDbType.VarChar, 32) { Value = entityType });
            command.Parameters.Add(new SqlParameter("@Cutoff", SqlDbType.DateTime2) { Value = cutoffUtc });

            batchDeleted = await command.ExecuteNonQueryAsync(cancellationToken);
            deleted += batchDeleted;
        }
        //A short batch means the last matching row is gone
        while (batchDeleted == batchSize);

        return deleted;
    }

    public async Task PingAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand("SELECT TOP 1 EntityType FROM ReactDataEntities", connection)
        {
            CommandTimeout = _connectionFactory.CommandTimeoutSeconds
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int count = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            count++;
        }

        Debug.Assert(count == 1 || count == 0);
    }
}
