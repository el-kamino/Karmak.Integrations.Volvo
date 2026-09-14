using Karmak.Integrations.Volvo.Common.Sql.Models;

namespace Karmak.Integrations.Volvo.Common.Sql;

public interface IReactDataRepository
{
    /// <summary>
    /// Inserts a new entity. <see cref="ReactDataEntity.Id"/> and
    /// <see cref="ReactDataEntity.CreatedOn"/> are populated when not supplied.
    /// </summary>
    Task CreateAsync<T>(ReactDataEntity<T> entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the stored payloads for a dealer and entity type, deserialized to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="entitityIds">Restricts the result to these entity ids. Null or empty applies no id filter.</param>
    /// <param name="windowStart">Exclusive lower bound on the row's <c>CreatedOn</c>. Null applies no lower bound.</param>
    /// <param name="windowEnd">Exclusive upper bound on the row's <c>CreatedOn</c>. Null applies no upper bound.</param>
    Task<List<T>> QueryAsync<T>(
        string paCode,
        string entityType,
        List<string> entitityIds,
        DateTime? windowStart = null,
        DateTime? windowEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes every row of one entity type whose <c>CreatedOn</c> is before <paramref name="cutoffUtc"/>,
    /// in batches of <paramref name="batchSize"/>, and returns the number of rows removed.
    /// </summary>
    /// <param name="cutoffUtc">Exclusive upper bound on the row's <c>CreatedOn</c>.</param>
    Task<int> DeleteOlderThanAsync(
        string entityType,
        DateTime cutoffUtc,
        int batchSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a database connection and attempts to read a single row from the ReactDataEntities table.
    /// </summary>
    Task PingAsync(CancellationToken cancellationToken = default);
}
