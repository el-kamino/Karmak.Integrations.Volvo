using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class InvoicedAfterReOpened : IVolvoEventEvaluator
    {
        public bool PreventsFurtherEvaluations { get; set; }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderIsReOpenedAccordingToTheLog(history))
            {
                PreventsFurtherEvaluations = true;
                if (RepairOrderIsInvoiced(ro))
                {
                    var newVolvoEvent = new Invoiced().OccurredAt(ro.SnapshotSequenceNumberDateTime);
                    history.AddPending(newVolvoEvent);
                }
            }
        }

        private static bool RepairOrderIsInvoiced(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED);
        }

        private bool RepairOrderIsReOpenedAccordingToTheLog(VolvoEventHistory history)
        {
            var lastInvoicedIndex = history.Events.FindLastIndex(state => state.Matches(Invoiced.DESCRIPTION));
            var lastReOpenedIndex = history.Events.FindLastIndex(state => state.Matches(ReOpened.DESCRIPTION));
            return lastReOpenedIndex > lastInvoicedIndex;
        }
    }
}