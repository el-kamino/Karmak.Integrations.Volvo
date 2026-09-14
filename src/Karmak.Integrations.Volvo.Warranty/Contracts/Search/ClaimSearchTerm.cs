using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Search
{
    /// <summary>
    /// One field-specific criterion of a <see cref="ClaimSearchRequest"/>.
    /// </summary>
    public class ClaimSearchTerm
    {
        /// <summary>
        /// Which field to match, named in camel case: "repairOrderIdentifier", "claimTotal",
        /// "repairOrderOpenedDate", and so on.
        /// </summary>
        [JsonProperty(PropertyName = "field")]
        public string Field { get; set; }

        /// <summary>
        /// One of "beginsWith", "contains", "equalTo", "notEqualTo", "greaterThan" or "lessThan".
        /// The first two match text only.
        /// </summary>
        [JsonProperty(PropertyName = "operator")]
        public string Operator { get; set; }

        /// <summary>
        /// The value to match, always sent as a string and read as the field's own type.
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
