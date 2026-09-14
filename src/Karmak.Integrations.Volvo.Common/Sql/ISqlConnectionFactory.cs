using Microsoft.Data.SqlClient;

namespace Karmak.Integrations.Volvo.Common.Sql;

public interface ISqlConnectionFactory
{
    /// <summary>
    /// Timeout to apply to commands created against connections from this factory.
    /// </summary>
    int CommandTimeoutSeconds { get; }

    /// <summary>
    /// Creates and opens a connection. The caller owns the connection and must dispose it.
    /// </summary>
    Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
