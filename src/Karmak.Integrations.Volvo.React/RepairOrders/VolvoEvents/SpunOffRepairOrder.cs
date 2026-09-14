using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class SpunOffRepairOrder : IVolvoEventEvaluator
    {
        public bool PreventsFurtherEvaluations { get; set; }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (HasReferenceToOriginalRepairOrder(ro) && IsInvoiced(ro) && (history.HasNoHistory() || IsSpunOffAndAlreadyInstantlyInvoiced(history)))
            {
                if (history.DoesNotContain(Invoiced.DESCRIPTION))
                {
                    history.AddPending(new AtDealership().OccurredAt(ro.SnapshotSequenceNumberDateTime));
                    history.AddPending(new Closed().OccurredAt(ro.SnapshotSequenceNumberDateTime));
                    history.AddPending(new Invoiced().OccurredAt(ro.InvoiceDate ?? ro.SnapshotSequenceNumberDateTime));
                }
                PreventsFurtherEvaluations = true;
            }
        }

        private bool IsSpunOffAndAlreadyInstantlyInvoiced(VolvoEventHistory history)
        {
            return history.Events.Count == 3 &&
                   history.Contains(AtDealership.DESCRIPTION) &&
                   history.Contains(Closed.DESCRIPTION) &&
                   history.Contains(Invoiced.DESCRIPTION);
        }

        private static bool HasReferenceToOriginalRepairOrder(RepairOrderSnapshot ro)
        {
            return !string.IsNullOrEmpty(ro.OriginalRepairOrderNumber);
        }

        private static bool IsInvoiced(RepairOrderSnapshot ro)
        {
            return ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED);
        }
    }
}