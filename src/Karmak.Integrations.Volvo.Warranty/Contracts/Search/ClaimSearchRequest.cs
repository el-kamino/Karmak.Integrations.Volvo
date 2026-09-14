using System.Collections.Generic;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Search
{
    /// <summary>
    /// A search of the claims stored for the caller's own dealer instance and branch, which are
    /// taken from the request's identity and cannot be named here.
    /// </summary>
    /// <remarks>
    /// A search runs in one of two modes and a request has to pick one: <see cref="Keyword"/>
    /// sweeps every text field at once, and <see cref="Terms"/> matches named fields individually.
    /// </remarks>
    public class ClaimSearchRequest
    {
        /// <summary>
        /// Matched anywhere within any text field, so "291" finds repair orders 291, 1291, 291234
        /// and 2182912 alike. Mutually exclusive with <see cref="Terms"/>.
        /// </summary>
        [JsonProperty(PropertyName = "keyword")]
        public string Keyword { get; set; }

        /// <summary>
        /// Criteria that each name their own field, combined with AND. A field can carry more than
        /// one term, so a range is written as two terms on the same field. Mutually exclusive with
        /// <see cref="Keyword"/>.
        /// </summary>
        [JsonProperty(PropertyName = "terms")]
        public IList<ClaimSearchTerm> Terms { get; set; }

        /// <summary>
        /// When false, which is the default, deleted claims are left out.
        /// </summary>
        [JsonProperty(PropertyName = "includeDeleted")]
        public bool IncludeDeleted { get; set; }

        /// <summary>
        /// When true, only claims still at the New status come back. Unlike a term this narrows a
        /// search in either mode, so a keyword can be swept across the new work and nothing else.
        /// </summary>
        [JsonProperty(PropertyName = "newOnly")]
        public bool NewOnly { get; set; }

        /// <summary>
        /// Field to sort by, named as in <see cref="ClaimSearchTerm.Field"/>. Left unset, results
        /// come back by repair order opened date with the newest first.
        /// </summary>
        [JsonProperty(PropertyName = "orderBy")]
        public string OrderBy { get; set; }

        /// <summary>
        /// Direction for <see cref="OrderBy"/>. Ignored when no field is named.
        /// </summary>
        [JsonProperty(PropertyName = "descending")]
        public bool Descending { get; set; }

        /// <summary>
        /// Results to skip before the page starts. Defaults to none.
        /// </summary>
        [JsonProperty(PropertyName = "skip")]
        public int Skip { get; set; }

        /// <summary>
        /// Results in the page. Defaults to 50 and is capped at 200.
        /// </summary>
        [JsonProperty(PropertyName = "top")]
        public int Top { get; set; }
    }
}
