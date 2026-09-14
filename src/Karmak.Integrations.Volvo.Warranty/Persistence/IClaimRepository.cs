using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Common.Sql.Models;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;

namespace Karmak.Integrations.Volvo.Warranty.Persistence
{
    /// <summary>
    /// Claim storage expressed as the handful of lookups warranty actually performs, rather
    /// than as an open query surface. Cosmos can answer an arbitrary predicate and sql cannot,
    /// so naming the lookups is what lets the two stores sit behind one interface.
    /// </summary>
    public interface IClaimRepository
    {
        Task<Claim> Create(Claim claim);

        Task<Claim> Update(Claim claim);

        /// <summary>
        /// Returns the claim with this id within the instance.
        /// </summary>
        Task<Claim> Find(string instanceIdentifier, string id);

        /// <summary>
        /// Undeleted claims raised against a warranty repair order number, regardless of which
        /// repair order or correlation group they belong to.
        /// </summary>
        Task<IEnumerable<Claim>> FindActiveByWarrantyRepairOrder(
            string instanceIdentifier,
            string branchIdentifier,
            string warrantyRepairOrderIdentifier);

        /// <summary>
        /// Undeleted claims for one repair order.
        /// </summary>
        Task<IEnumerable<Claim>> FindActiveByRepairOrder(string instanceIdentifier, RepairOrderKey repairOrder);

        /// <summary>
        /// Undeleted claims for one repair order that belong to the given correlation group.
        /// </summary>
        Task<IEnumerable<Claim>> FindActiveInCorrelation(
            string instanceIdentifier,
            RepairOrderKey repairOrder,
            string correlationId);

        /// <summary>
        /// Undeleted claims for one repair order that belong to some other correlation group,
        /// including those with no correlation group at all.
        /// </summary>
        Task<IEnumerable<Claim>> FindActiveOutsideCorrelation(
            string instanceIdentifier,
            RepairOrderKey repairOrder,
            string correlationId);

        /// <summary>
        /// One page of the claims matching an open-ended search, newest first unless the query says
        /// otherwise, with the size of the whole result set alongside it.
        /// </summary>
        /// <remarks>
        /// The one place the named lookups above give way to a query the caller composes, because a
        /// search box is exactly the case they cannot cover. It is still not an open surface: the
        /// query can only name fields the store has promoted into columns, and it returns those
        /// columns rather than claims, so it stays a search rather than a way to read claims in bulk.
        /// </remarks>
        Task<ClaimSearchResponse> Search(WarrantyClaimSearchQuery query);
    }
}
