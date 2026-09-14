namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// A claim as a search result: the promoted columns and nothing else.
/// </summary>
/// <remarks>
/// Its own type rather than a <see cref="WarrantyClaimEntity"/> with a null JsonData, so a
/// summary can never be mistaken for a claim whose document failed to load. Search reads pages of
/// rows and the json column holds the whole claim graph, so leaving it out of the select is the
/// difference between a page of kilobytes and a page of megabytes.
/// </remarks>
public class WarrantyClaimSummary
{
    public string ClaimId { get; set; }
    public string InstanceIdentifier { get; set; }
    public string BranchIdentifier { get; set; }

    /// <summary>
    /// The branch's own code, as a dealer knows it. Promoted alongside the identifier because the
    /// code is what a result list shows, while the identifier is what a search is scoped by.
    /// </summary>
    public string BranchCode { get; set; }
    public string CorrelationId { get; set; }
    public bool IsDeleted { get; set; }

    public string CausalPartIdentifier { get; set; }
    public string ClaimIdentifier { get; set; }
    public string CompanyName { get; set; }
    public decimal? ClaimTotal { get; set; }
    public DateTime? RepairOrderCompletedDate { get; set; }
    public DateTime? RepairOrderOpenedDate { get; set; }
    public string CustomerIdentifier { get; set; }
    public string InvoiceIdentifier { get; set; }
    public string Oem { get; set; }
    public string RepairOrderIdentifier { get; set; }
    public string WarrantyRepairOrderIdentifier { get; set; }
    public string ClaimStatus { get; set; }
    public string VehicleIdentifier { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
