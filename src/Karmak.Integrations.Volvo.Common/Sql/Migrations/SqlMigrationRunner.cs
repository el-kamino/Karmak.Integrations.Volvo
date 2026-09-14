using System.Data;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Common.Sql.Migrations;

/// <summary>
/// Applies the embedded update scripts under Sql/Migrations/Scripts, recording each one in
/// [dbo].[SchemaVersions] so it is only ever applied once.
/// </summary>
public class SqlMigrationRunner : ISqlMigrationRunner
{
    private const string ScriptResourcePrefix = "Karmak.Integrations.Volvo.Common.Sql.Migrations.Scripts.";
    private const string ScriptResourceSuffix = ".sql";

    //Serializes migration across instances that start at the same time
    private const string AppLockResourceName = "Karmak.Integrations.Volvo.Sql.Migrations";

    private const string EnsureJournalSql = @"
IF OBJECT_ID(N'[dbo].[SchemaVersions]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[SchemaVersions]
    (
        [Id]         INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SchemaVersions] PRIMARY KEY,
        [ScriptName] NVARCHAR(255) NOT NULL CONSTRAINT [UQ_SchemaVersions_ScriptName] UNIQUE,
        [AppliedOn]  DATETIME2(3) NOT NULL CONSTRAINT [DF_SchemaVersions_AppliedOn] DEFAULT SYSUTCDATETIME()
    );
END";

    private static readonly Assembly ScriptAssembly = typeof(SqlMigrationRunner).Assembly;

    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly SqlDataLayerOptions _options;
    private readonly ILogger<SqlMigrationRunner> _logger;

    public SqlMigrationRunner(
        ISqlConnectionFactory connectionFactory,
        IOptions<SqlDataLayerOptions> options,
        ILogger<SqlMigrationRunner> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<string>> ApplyPendingScriptsAsync(CancellationToken cancellationToken = default)
    {
        var scriptNames = GetScriptNames();

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await EnsureJournalTableAsync(connection, cancellationToken);
        await AcquireLockAsync(connection, cancellationToken);

        try
        {
            var applied = await GetAppliedScriptNamesAsync(connection, cancellationToken);
            var pending = scriptNames.Where(name => !applied.Contains(name)).ToList();

            if (pending.Count == 0)
            {
                _logger.LogInformation("Sql schema is up to date; {ScriptCount} script(s) already applied.", applied.Count);
                return Array.Empty<string>();
            }

            _logger.LogInformation("Applying {PendingCount} pending sql script(s): {Scripts}", pending.Count, string.Join(", ", pending));

            foreach (var scriptName in pending)
            {
                await ApplyScriptAsync(connection, scriptName, cancellationToken);
                _logger.LogInformation("Applied sql script {ScriptName}.", scriptName);
            }

            return pending;
        }
        finally
        {
            await ReleaseLockAsync(connection);
        }
    }

    /// <summary>
    /// Embedded script names, ordered so the numeric prefix drives execution order.
    /// </summary>
    private static List<string> GetScriptNames()
    {
        return ScriptAssembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ScriptResourcePrefix, StringComparison.Ordinal)
                && name.EndsWith(ScriptResourceSuffix, StringComparison.OrdinalIgnoreCase))
            .Select(name => name.Substring(ScriptResourcePrefix.Length))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static async Task<string> ReadScriptAsync(string scriptName)
    {
        var resourceName = ScriptResourcePrefix + scriptName;

        await using var stream = ScriptAssembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded sql script '{resourceName}' could not be found.");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task EnsureJournalTableAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        await using var command = CreateCommand(EnsureJournalSql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<HashSet<string>> GetAppliedScriptNamesAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        var applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var command = CreateCommand("SELECT [ScriptName] FROM [dbo].[SchemaVersions];", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            applied.Add(reader.GetString(0));
        }

        return applied;
    }

    /// <summary>
    /// Runs one script and journals it in a single transaction, so a failure part way
    /// through leaves the script pending rather than half applied.
    /// </summary>
    private async Task ApplyScriptAsync(SqlConnection connection, string scriptName, CancellationToken cancellationToken)
    {
        var script = await ReadScriptAsync(scriptName);

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var batch in SplitBatches(script))
            {
                await using var command = CreateCommand(batch, connection, transaction);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await using (var journalCommand = CreateCommand(
                "INSERT INTO [dbo].[SchemaVersions] ([ScriptName]) VALUES (@ScriptName);", connection, transaction))
            {
                journalCommand.Parameters.Add(new SqlParameter("@ScriptName", SqlDbType.NVarChar, 255) { Value = scriptName });
                await journalCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _logger.LogError(exception, "Sql script {ScriptName} failed and was rolled back.", scriptName);
            throw;
        }
    }

    /// <summary>
    /// Splits a script on GO separators, which are a client-side convention the driver
    /// does not understand.
    /// </summary>
    private static IEnumerable<string> SplitBatches(string script)
    {
        var batches = script
            .Split(["\r\n", "\n"], StringSplitOptions.None)
            .Aggregate(new List<List<string>> { new() }, (accumulator, line) =>
            {
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    accumulator.Add([]);
                }
                else
                {
                    accumulator[^1].Add(line);
                }

                return accumulator;
            })
            .Select(lines => string.Join(Environment.NewLine, lines));

        return batches.Where(batch => !string.IsNullOrWhiteSpace(batch));
    }

    private async Task AcquireLockAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        //Session owned: the lock outlives the individual per-script transactions below
        await using var command = CreateCommand("sp_getapplock", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = _options.MigrationLockTimeoutSeconds + _options.CommandTimeoutSeconds;

        command.Parameters.Add(new SqlParameter("@Resource", SqlDbType.NVarChar, 255) { Value = AppLockResourceName });
        command.Parameters.Add(new SqlParameter("@LockMode", SqlDbType.NVarChar, 32) { Value = "Exclusive" });
        command.Parameters.Add(new SqlParameter("@LockOwner", SqlDbType.NVarChar, 32) { Value = "Session" });
        command.Parameters.Add(new SqlParameter("@LockTimeout", SqlDbType.Int) { Value = _options.MigrationLockTimeoutSeconds * 1000 });

        var returnValue = new SqlParameter { Direction = ParameterDirection.ReturnValue, SqlDbType = SqlDbType.Int };
        command.Parameters.Add(returnValue);

        await command.ExecuteNonQueryAsync(cancellationToken);

        //0 = granted, 1 = granted after waiting; anything else failed
        var result = (int)returnValue.Value;

        if (result is not (0 or 1))
        {
            throw new InvalidOperationException(
                $"Could not acquire the sql migration lock '{AppLockResourceName}' (sp_getapplock returned {result}).");
        }
    }

    private async Task ReleaseLockAsync(SqlConnection connection)
    {
        if (connection.State != ConnectionState.Open)
        {
            //The lock is session scoped, so a dropped connection has already released it
            return;
        }

        try
        {
            await using var command = CreateCommand("sp_releaseapplock", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@Resource", SqlDbType.NVarChar, 255) { Value = AppLockResourceName });
            command.Parameters.Add(new SqlParameter("@LockOwner", SqlDbType.NVarChar, 32) { Value = "Session" });

            await command.ExecuteNonQueryAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            //Never mask the original failure; closing the connection releases the lock anyway
            _logger.LogWarning(exception, "Failed to release the sql migration lock '{Resource}'.", AppLockResourceName);
        }
    }

    private SqlCommand CreateCommand(string commandText, SqlConnection connection, SqlTransaction transaction = null)
    {
        return new SqlCommand(commandText, connection, transaction)
        {
            CommandTimeout = _options.MigrationCommandTimeoutSeconds
        };
    }
}
