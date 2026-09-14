using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class VehicleCompleted : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "VEHICLE_COMPLETED";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public VehicleCompleted() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (AllTasksCompleted(ro) && history.DoesNotContain(this))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private static bool AllTasksCompleted(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.Tasks.Any() && repairOrder.Tasks.All(task => task.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.CLOSED));
        }
    }
}