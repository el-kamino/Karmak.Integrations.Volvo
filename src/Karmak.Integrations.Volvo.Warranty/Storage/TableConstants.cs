using System.Collections.Immutable;

namespace Karmak.Integrations.Volvo.Warranty.Storage
{
    public static class TableConstants
    {
        public const string CLAIM_JOB_CORRELATION_TABLE = "VolvoWarrantyClaimCorrelation";
        public static readonly ImmutableArray<string> TABLES = ImmutableArray.Create(CLAIM_JOB_CORRELATION_TABLE);
    }
}
