using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    /// <summary>
    /// Converts between the claim contract and the sql row that stores it.
    /// </summary>
    public interface IWarrantyClaimEntityMapper
    {
        /// <summary>
        /// Serializes the claim and promotes the searchable values out of it.
        /// </summary>
        WarrantyClaimEntity ToEntity(Claim claim);

        /// <summary>
        /// Rehydrates the claim from the row's json payload. Returns null for a null row.
        /// </summary>
        Claim ToClaim(WarrantyClaimEntity entity);

        /// <summary>
        /// Presents a searched row as a result. Reads the promoted columns only, so it never opens
        /// the claim document. Returns null for a null row.
        /// </summary>
        ClaimSearchResult ToSearchResult(WarrantyClaimSummary summary);
    }
}
