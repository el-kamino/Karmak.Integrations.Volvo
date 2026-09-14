using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class Canceled : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "CANCELED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations { get; set; }

        public Canceled() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            // new ro status is voided & has not already been canceled
            if (RepairOrderIsVoided(ro) && history.DoesNotContain(this))
            {
                PreventsFurtherEvaluations = true;

                // if currently invoiced, append a reopened event since an invoiced ro can't be canceled
                if (RepairOrderIsInvoicedAccordingToTheLog(history))
                    history.AddPending(new ReOpened().OccurredAt(ro.SnapshotSequenceNumberDateTime));

                // send closed status instead of canceled since record exists in OEM system
                if (RepairOrderWasEverInvoiced(history))
                    Status = Closed.DESCRIPTION;

                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private bool RepairOrderIsVoided(RepairOrderSnapshot ro) =>
            ro.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.VOIDED);

        private bool RepairOrderWasEverInvoiced(VolvoEventHistory history) =>
            history.Events.Exists(state => state.Matches(Invoiced.DESCRIPTION));
    }
}