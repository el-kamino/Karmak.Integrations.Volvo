namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// One field-specific criterion. Every term in a search is ANDed, and a field can carry more than
/// one of them, so a range is written as two terms naming the same field.
/// </summary>
public class WarrantyClaimSearchTerm
{
    public WarrantyClaimField Field { get; set; }

    public WarrantyClaimSearchOperator Operator { get; set; }

    /// <summary>
    /// The value to compare against, already parsed to the field's own type: a string for the
    /// text fields, a decimal for ClaimTotal, a DateTime for the date fields. Callers parse
    /// because they are the ones holding the raw input and able to report where it went wrong.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Set when <see cref="Value"/> names a day rather than a moment within one, which is how anyone
    /// filtering on a date writes it. The day is then compared as the whole of itself, so "equal to
    /// the second of March" is every claim from that day rather than the one stored at midnight.
    /// </summary>
    public bool DateOnly { get; set; }
}
