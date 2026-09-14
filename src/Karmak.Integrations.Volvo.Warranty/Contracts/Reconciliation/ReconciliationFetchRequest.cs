using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts.Reconciliation
{
    public class ReconciliationFetchRequest
    {
        public string PACode { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
