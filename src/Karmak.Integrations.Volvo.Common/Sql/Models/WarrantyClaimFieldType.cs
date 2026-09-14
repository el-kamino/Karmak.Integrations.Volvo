namespace Karmak.Integrations.Volvo.Common.Sql.Models;

/// <summary>
/// What a <see cref="WarrantyClaimField"/> holds. Decides which operators apply to it and what a
/// term's value has to be parsed into before it can be compared.
/// </summary>
public enum WarrantyClaimFieldType
{
    Text,
    Number,
    Date
}
