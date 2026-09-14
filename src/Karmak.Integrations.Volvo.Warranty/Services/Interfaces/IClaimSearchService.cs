using System.Threading.Tasks;
using Karmak.Integrations.Volvo.Warranty.Contracts.Search;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    /// <summary>
    /// Searches the claims stored in sql. Named apart from <see cref="IClaimsSearchService"/>, which
    /// serves the same question out of the azure search index.
    /// </summary>
    public interface IClaimSearchService
    {
        /// <summary>
        /// Runs the search against the caller's own dealer instance and branch.
        /// </summary>
        /// <exception cref="Contracts.Exceptions.InvalidClaimSearchException">
        /// The request names a field or operator that does not exist, gives a value the field cannot
        /// hold, or asks for both search modes at once.
        /// </exception>
        Task<ClaimSearchResponse> Search(ClaimSearchRequest request);
    }
}
