namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// One warranty claim row: the claim document itself in <see cref="JsonData"/>, plus the
/// columns promoted out of it so callers can search without opening the json.
/// </summary>
/// <remarks>
/// <see cref="JsonData"/> is deliberately a string. The claim contract is decorated for
/// Newtonsoft (string enum converters, a remapped id property, non-public setters), so the
/// owning module serializes it and this layer only stores what it is handed.
/// </remarks>
public class WarrantyClaimEntity
{
    /// <summary>
    /// The claim's own identifier. Unique within <see cref="InstanceIdentifier"/>.
    /// </summary>
    public string ClaimId { get; set; }

    /// <summary>
    /// Dealer instance the claim belongs to. This is the cosmos partition key, kept as the
    /// leading key column so lookups stay scoped to a single dealer.
    /// </summary>
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

    /// <summary>
    /// The repair order's secondary identifier, which warranty treats as the repair order
    /// number claims are looked up by.
    /// </summary>
    public string WarrantyRepairOrderIdentifier { get; set; }

    public string ClaimStatus { get; set; }
    public string VehicleIdentifier { get; set; }

    /// <summary>
    /// When the row first landed in sql. Set on insert and preserved by later upserts, so it
    /// records the migration or creation time rather than the claim's own audit stamp.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// When the row was last written.
    /// </summary>
    public DateTime UpdatedOn { get; set; }

    /// <summary>
    /// The serialized claim document.
    /// </summary>
    public string JsonData { get; set; }
}
