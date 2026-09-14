namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class UpdateSnapshotMessage
    {
        public UpdateSnapshot UpdateSnapshot { get; set; }
        public string ClaimId { get; set; }
        public string TransactionId { get; set; }
    }
}
