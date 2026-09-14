namespace Karmak.Integrations.Volvo.Common.Sql.Migrations;

public interface ISqlMigrationRunner
{
    /// <summary>
    /// Applies every embedded update script that this database has not recorded yet, in
    /// script-name order. Safe to call concurrently from multiple instances.
    /// </summary>
    /// <returns>The names of the scripts applied by this call.</returns>
    Task<IReadOnlyList<string>> ApplyPendingScriptsAsync(CancellationToken cancellationToken = default);
}
