using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class Closed : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "CLOSED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public Closed() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderIsClosed(ro) && history.DoesNotContain(this))
            {
                if (ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.CLOSED))
                    OccurredAt(ro.SnapshotSequenceNumberDateTime);
                if (ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED))
                    OccurredAt(ro.InvoiceDate.Value);
                history.AddPending(this);
            }
        }

        private bool RepairOrderIsClosed(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.CLOSED) ||
                   repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED);
        }
    }
}