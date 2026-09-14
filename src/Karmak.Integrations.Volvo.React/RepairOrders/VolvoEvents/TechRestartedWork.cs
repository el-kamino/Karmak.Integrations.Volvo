using System.Linq;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class TechRestartedWork : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "TECHNICIAN_RESTARTS_WORK";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public TechRestartedWork() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (RepairOrderHasOpenTasks(ro) && WorkIsStopped(history))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private bool RepairOrderHasOpenTasks(RepairOrderSnapshot ro)
        {
            return ro.Tasks.Any(t =>
            {
                return !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.HOLD) &&
                       !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.WAITING_FOR_PARTS);
            });
        }

        private bool WorkIsStopped(VolvoEventHistory history)
        {
            var lastWorkStoppage = LastIndexOfWorkStopped(history);
            var lastWorkRestarted = history.Events.FindLastIndex(volvoEvent => volvoEvent.Matches(DESCRIPTION));
            return lastWorkStoppage > lastWorkRestarted;
        }

        private int LastIndexOfWorkStopped(VolvoEventHistory history)
        {
            return history.Events.FindLastIndex(volvoEvent => volvoEvent.Matches(TechStoppedWork.DESCRIPTION));
        }
    }
}