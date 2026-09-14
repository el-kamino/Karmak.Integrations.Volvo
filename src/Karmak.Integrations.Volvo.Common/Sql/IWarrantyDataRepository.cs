using Karmak.Integrations.Volvo.Common.Sql.Models;

namespace Karmak.Integrations.Volvo.Common.Sql;

public interface IWarrantyDataRepository
{
    /// <summary>
    /// Inserts the claim, or replaces it when a row with the same
    /// <see cref="WarrantyClaimEntity.InstanceIdentifier"/> and
    /// <see cref="WarrantyClaimEntity.ClaimId"/> already exists. Idempotent, so a migration
    /// that is run twice produces the same table as one that is run once.
    /// </summary>
    Task UpsertAsync(WarrantyClaimEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single claim, or null when the instance holds no claim with that id.
    /// </summary>
    Task<WarrantyClaimEntity> FindAsync(string instanceIdentifier, string claimId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every claim in the instance matching the supplied criteria.
    /// </summary>
    Task<List<WarrantyClaimEntity>> QueryAsync(WarrantyClaimQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns one page of the claims matching the search, together with the size of the whole
    /// result set. Results carry the promoted columns only; use <see cref="FindAsync"/> to open the
    /// document behind one of them.
    /// </summary>
    Task<WarrantyClaimSearchPage> SearchAsync(WarrantyClaimSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Number of claims stored for an instance. Used to report migration progress.
    /// </summary>
    Task<int> CountAsync(string instanceIdentifier, CancellationToken cancellationToken = default);
}
