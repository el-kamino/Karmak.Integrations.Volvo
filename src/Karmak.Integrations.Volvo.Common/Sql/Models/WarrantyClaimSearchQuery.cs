namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// A page of a claim search. Searching happens in one of two modes: a single
/// <see cref="Keyword"/> matched loosely against every text column, or a set of
/// <see cref="Terms"/> that each name their own field. Supplying both is not meaningful and the
/// builder rejects it.
/// </summary>
public class WarrantyClaimSearchQuery
{
    /// <summary>
    /// Dealer instance to search within. Required, so a search can never span dealers.
    /// </summary>
    public string InstanceIdentifier { get; set; }

    /// <summary>
    /// Branch to search within. Optional only so a background caller can search the whole
    /// instance; requests arriving over http always carry the caller's own branch.
    /// </summary>
    public string BranchIdentifier { get; set; }

    /// <summary>
    /// When false, which is the default, deleted claims are left out.
    /// </summary>
    public bool IncludeDeleted { get; set; }

    /// <summary>
    /// When true, only claims still sitting at the New status come back. Unlike a term, this narrows
    /// a search in either mode, so a caller can sweep a keyword across the new work waiting on them.
    /// </summary>
    public bool NewOnly { get; set; }

    /// <summary>
    /// Matched as "contains" against every text column at once. Mutually exclusive with
    /// <see cref="Terms"/>.
    /// </summary>
    public string Keyword { get; set; }

    /// <summary>
    /// Field-specific criteria, combined with AND. Mutually exclusive with <see cref="Keyword"/>.
    /// </summary>
    public IReadOnlyList<WarrantyClaimSearchTerm> Terms { get; set; }

    /// <summary>
    /// Column to order by. Defaults to the repair order's opened date when not set.
    /// </summary>
    public WarrantyClaimField? SortField { get; set; }

    public bool SortDescending { get; set; }

    /// <summary>
    /// Rows to skip before the page starts.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Rows in the page. Must be greater than zero.
    /// </summary>
    public int Take { get; set; }
}
