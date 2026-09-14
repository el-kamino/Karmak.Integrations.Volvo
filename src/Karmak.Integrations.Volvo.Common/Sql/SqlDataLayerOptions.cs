namespace Karmak.Integrations.Volvo.Common.Sql;

public class SqlDataLayerOptions
{
    /// <summary>
    /// Connection string for the Azure SQL database.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Timeout applied to every command issued by the data layer.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Timeout applied to migration commands, which can run longer than normal queries.
    /// </summary>
    public int MigrationCommandTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// How long to wait for the migration lock held by another starting instance.
    /// </summary>
    public int MigrationLockTimeoutSeconds { get; set; } = 120;
}
