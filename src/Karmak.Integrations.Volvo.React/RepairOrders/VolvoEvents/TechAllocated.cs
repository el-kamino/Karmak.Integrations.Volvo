using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class TechAllocated : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "TECHNICIAN_ALLOCATED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public TechAllocated() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (HasLaborEntries(ro) && history.DoesNotContain(this))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private bool HasLaborEntries(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.Tasks.Any(t => t.LaborEntries.Any());
        }
    }
}