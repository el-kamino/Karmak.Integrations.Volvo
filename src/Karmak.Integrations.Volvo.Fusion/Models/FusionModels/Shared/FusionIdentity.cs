namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared
{
    public class FusionIdentity {
        public string AccountCode { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }

        public static readonly FusionIdentity SystemUser = new FusionIdentity
        {
            Username = "Fusion",
            AccountCode = null,
            BranchCode = null
        };

        public static readonly FusionIdentity VolvoRecon = new FusionIdentity
        {
            Username = "VolvoRecon",
            AccountCode = null,
            BranchCode = null
        };

        public static readonly FusionIdentity VolvoStatus = new FusionIdentity
        {
            Username = "VolvoStatus",
            AccountCode = null,
            BranchCode = null
        };

        public static readonly FusionIdentity Volvo = new FusionIdentity
        {
            Username = "Volvo",
            AccountCode = null,
            BranchCode = null
        };
    }
}
