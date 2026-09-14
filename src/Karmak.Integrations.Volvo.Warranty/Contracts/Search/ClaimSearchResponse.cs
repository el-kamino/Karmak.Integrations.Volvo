using System.Collections.Generic;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Search
{
    /// <summary>
    /// One page of search results, with the size of the whole result set so a caller can page
    /// without asking for a count of its own.
    /// </summary>
    public class ClaimSearchResponse
    {
        /// <summary>
        /// Claims matching the search across every page, not the number in <see cref="Results"/>.
        /// </summary>
        [JsonProperty(PropertyName = "totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty(PropertyName = "skip")]
        public int Skip { get; set; }

        [JsonProperty(PropertyName = "top")]
        public int Top { get; set; }

        [JsonProperty(PropertyName = "results")]
        public IList<ClaimSearchResult> Results { get; set; } = new List<ClaimSearchResult>();
    }
}
