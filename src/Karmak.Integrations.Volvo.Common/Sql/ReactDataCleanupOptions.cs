namespace Karmak.Integrations.Volvo.Common.Sql;

public class ReactDataCleanupOptions
{
    /// <summary>
    /// Opt in per environment. Nothing is deleted while this is false.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Time of day, UTC, at which the cleanup runs.
    /// </summary>
    public TimeSpan RunAtUtc { get; set; } = TimeSpan.FromHours(7);

    /// <summary>
    /// Rows whose <c>CreatedOn</c> is older than this many days are deleted.
    /// </summary>
    public int RetentionDays { get; set; } = 180;

    /// <summary>
    /// Rows removed per delete statement. Keeps each statement short enough that a large
    /// purge does not block the dispatchers still inserting.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    public TimeSpan RetentionPeriod => TimeSpan.FromDays(RetentionDays);
}
