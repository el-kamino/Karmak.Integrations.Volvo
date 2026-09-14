using System;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Search
{
    /// <summary>
    /// One claim as a search result. Carries what a result list shows; the claim itself is read
    /// from the claims endpoint by <see cref="ClaimId"/>.
    /// </summary>
    public class ClaimSearchResult
    {
        [JsonProperty(PropertyName = "claimId")]
        public string ClaimId { get; set; }

        [JsonProperty(PropertyName = "branchIdentifier")]
        public string BranchIdentifier { get; set; }

        /// <summary>
        /// The branch as a dealer names it. The identifier above is what the search was scoped by.
        /// </summary>
        [JsonProperty(PropertyName = "branchCode")]
        public string BranchCode { get; set; }

        [JsonProperty(PropertyName = "correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty(PropertyName = "isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty(PropertyName = "causalPartIdentifier")]
        public string CausalPartIdentifier { get; set; }

        [JsonProperty(PropertyName = "claimIdentifier")]
        public string ClaimIdentifier { get; set; }

        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "claimTotal")]
        public decimal? ClaimTotal { get; set; }

        [JsonProperty(PropertyName = "repairOrderCompletedDate")]
        public DateTime? RepairOrderCompletedDate { get; set; }

        [JsonProperty(PropertyName = "repairOrderOpenedDate")]
        public DateTime? RepairOrderOpenedDate { get; set; }

        [JsonProperty(PropertyName = "customerIdentifier")]
        public string CustomerIdentifier { get; set; }

        [JsonProperty(PropertyName = "invoiceIdentifier")]
        public string InvoiceIdentifier { get; set; }

        [JsonProperty(PropertyName = "oem")]
        public string Oem { get; set; }

        [JsonProperty(PropertyName = "repairOrderIdentifier")]
        public string RepairOrderIdentifier { get; set; }

        [JsonProperty(PropertyName = "warrantyRepairOrderIdentifier")]
        public string WarrantyRepairOrderIdentifier { get; set; }

        [JsonProperty(PropertyName = "claimStatus")]
        public string ClaimStatus { get; set; }

        [JsonProperty(PropertyName = "vehicleIdentifier")]
        public string VehicleIdentifier { get; set; }

        [JsonProperty(PropertyName = "createdOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty(PropertyName = "updatedOn")]
        public DateTime UpdatedOn { get; set; }
    }
}
