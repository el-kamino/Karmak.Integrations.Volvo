using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class ReOpened : VolvoEventWithoutTransmission, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "CLOSED_REOPENED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations { get; set; }

        public ReOpened() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderIsNotInvoicedOrVoided(ro) && RepairOrderIsInvoicedAccordingToTheLog(history))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                PreventsFurtherEvaluations = true;
                history.AddPending(this);
            }
        }

        private bool RepairOrderIsNotInvoicedOrVoided(RepairOrderSnapshot ro)
        {
            return !ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED)
                && !ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.VOIDED);
        }
    }
}