using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Utils;
using Newtonsoft.Json;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class TechStoppedWork : VolvoEvent, IVolvoEventEvaluator
    {
        public const string DESCRIPTION = "TECHNICIAN_STOPS_WORK";
        [JsonIgnore]
        public bool PreventsFurtherEvaluations => false;

        public TechStoppedWork() : base(DESCRIPTION) { }

        public void Evaluate(RepairOrderSnapshot ro, VolvoEventHistory history)
        {
            if (AllTasksAreOnHold(ro) && TechIsNotCurrentlyStopped(history))
            {
                OccurredAt(ro.SnapshotSequenceNumberDateTime);
                history.AddPending(this);
            }
        }

        private static bool AllTasksAreOnHold(RepairOrderSnapshot ro)
        {
            var incompleteTasks = ro.Tasks
                .Where(t => !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.CLOSED))
                .ToList();

            return incompleteTasks.Any() && incompleteTasks
                       .All(t => t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.HOLD) ||
                                 t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.WAITING_FOR_PARTS));
        }

        private bool TechIsNotCurrentlyStopped(VolvoEventHistory history)
        {
            return TechHadStartedWork(history) && (WorkNeverStopped(history) || WorkRestartedSinceLastStoppage(history));
        }

        private bool TechHadStartedWork(VolvoEventHistory history)
        {
            return history.Contains(new TechAllocated());
        }

        private bool WorkNeverStopped(VolvoEventHistory history)
        {
            return history.DoesNotContain(DESCRIPTION);
        }

        private bool WorkRestartedSinceLastStoppage(VolvoEventHistory history)
        {
            var lastWorkStoppage = history.Events.FindLastIndex(volvoEvent => volvoEvent.Matches(DESCRIPTION));
            var lastWorkRestarted = history.Events.FindLastIndex(volvoEvent => volvoEvent.Matches(TechRestartedWork.DESCRIPTION));
            return lastWorkRestarted > lastWorkStoppage;
        }
    }
}