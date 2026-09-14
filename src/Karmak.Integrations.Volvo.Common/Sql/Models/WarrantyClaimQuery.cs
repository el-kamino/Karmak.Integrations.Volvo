namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// Search criteria over the promoted claim columns. Every property except
/// <see cref="InstanceIdentifier"/> is optional; the ones left null are not filtered on.
/// </summary>
public class WarrantyClaimQuery
{
    /// <summary>
    /// Dealer instance to search within. Required.
    /// </summary>
    public string InstanceIdentifier { get; set; }

    public string BranchIdentifier { get; set; }

    /// <summary>
    /// Correlation group to match. Interpreted by <see cref="ExcludeCorrelationId"/>.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// When true, <see cref="CorrelationId"/> excludes rather than includes: rows in that
    /// correlation group are filtered out and rows with no correlation id are kept. This
    /// mirrors how the cosmos queries treat an inequality against a missing property.
    /// </summary>
    public bool ExcludeCorrelationId { get; set; }

    public bool? IsDeleted { get; set; }

    public string CausalPartIdentifier { get; set; }
    public string ClaimIdentifier { get; set; }
    public string CompanyName { get; set; }
    public string CustomerIdentifier { get; set; }
    public string InvoiceIdentifier { get; set; }
    public string Oem { get; set; }
    public string RepairOrderIdentifier { get; set; }
    public string WarrantyRepairOrderIdentifier { get; set; }
    public string ClaimStatus { get; set; }
    public string VehicleIdentifier { get; set; }

    /// <summary>
    /// Inclusive lower bound on the claim total.
    /// </summary>
    public decimal? ClaimTotalFrom { get; set; }

    /// <summary>
    /// Inclusive upper bound on the claim total.
    /// </summary>
    public decimal? ClaimTotalTo { get; set; }

    /// <summary>
    /// Inclusive lower bound on the repair order's opened date.
    /// </summary>
    public DateTime? RepairOrderOpenedFrom { get; set; }

    /// <summary>
    /// Inclusive upper bound on the repair order's opened date.
    /// </summary>
    public DateTime? RepairOrderOpenedTo { get; set; }

    /// <summary>
    /// Inclusive lower bound on the repair order's completed date.
    /// </summary>
    public DateTime? RepairOrderCompletedFrom { get; set; }

    /// <summary>
    /// Inclusive upper bound on the repair order's completed date.
    /// </summary>
    public DateTime? RepairOrderCompletedTo { get; set; }
}
