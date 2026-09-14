namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// One page of search results together with the size of the whole result set, so a caller can
/// page without issuing a second count query.
/// </summary>
public class WarrantyClaimSearchPage
{
    public List<WarrantyClaimSummary> Items { get; set; } = [];

    /// <summary>
    /// Rows matching the search across every page, not the number in <see cref="Items"/>.
    /// </summary>
    public int TotalCount { get; set; }
}
