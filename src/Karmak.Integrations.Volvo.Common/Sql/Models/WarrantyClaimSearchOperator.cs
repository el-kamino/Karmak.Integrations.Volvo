namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// How a <see cref="WarrantyClaimSearchTerm"/> compares its value to the column.
/// </summary>
public enum WarrantyClaimSearchOperator
{
    /// <summary>Text only. Matches values starting with the term.</summary>
    BeginsWith,

    /// <summary>Text only. Matches values holding the term anywhere, which is the fuzzy match.</summary>
    Contains,

    EqualTo,

    /// <summary>Also matches rows whose column is null, since a missing value is not the term.</summary>
    NotEqualTo,

    GreaterThan,

    LessThan
}
