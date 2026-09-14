using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class Invoiced : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "INVOICED";
        public const string STATUS = "CLOSED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public Invoiced() : base(DESCRIPTION, STATUS) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderIsInvoiced(ro) && history.DoesNotContain(this))
            {
                OccurredAt(ro.InvoiceDate ?? ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private static bool RepairOrderIsInvoiced(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED);
        }
    }
}