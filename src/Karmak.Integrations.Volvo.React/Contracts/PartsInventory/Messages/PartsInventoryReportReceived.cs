using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;

namespace Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Messages
{
    public class PartsInventoryReportReceived 
    {
        public PartsInventoryReportClaimCheck ReportClaimCheck { get; set; }
    }
}