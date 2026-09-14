using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class AtDealership : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "AT_DEALERSHIP/AWAITING_TECHNICIAN";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public AtDealership() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderIsAtDealership(ro) && history.DoesNotContain(this))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private static bool RepairOrderIsAtDealership(RepairOrderSnapshot ro)
        {
            return ro.SubStatus.EqualsIgnoreCase(RepairOrderSubstatus.AT_DEALERSHIP);
        }
    }
}