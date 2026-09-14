using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Common.Sql;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private const int DefaultConnectRetryCount = 3;
    private const int DefaultConnectRetryIntervalSeconds = 10;

    private readonly string _connectionString;

    public int CommandTimeoutSeconds { get; }

    public SqlConnectionFactory(IOptions<SqlDataLayerOptions> options)
    {
        var value = options?.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(value.ConnectionString))
        {
            throw new ArgumentException("A SQL connection string is required.", nameof(options));
        }

        CommandTimeoutSeconds = value.CommandTimeoutSeconds;
        _connectionString = BuildConnectionString(value.ConnectionString);
    }

    public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }

        return connection;
    }

    /// <summary>
    /// Azure SQL drops idle and throttled connections. Opting into the driver's connection
    /// resiliency keeps those transient faults from surfacing to callers, unless the
    /// configured connection string already states its own preference.
    /// </summary>
    private static string BuildConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!builder.ContainsKey("ConnectRetryCount"))
        {
            builder.ConnectRetryCount = DefaultConnectRetryCount;
        }

        if (!builder.ContainsKey("ConnectRetryInterval"))
        {
            builder.ConnectRetryInterval = DefaultConnectRetryIntervalSeconds;
        }

        return builder.ConnectionString;
    }
}
