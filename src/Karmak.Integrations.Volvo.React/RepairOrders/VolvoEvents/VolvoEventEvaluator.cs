using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents
{
    public class VolvoEventEvaluator
    {
        public readonly IEnumerable<IVolvoEventEvaluator> PossibleVolvoEvents = new List<IVolvoEventEvaluator> {
            new Canceled(),
            new InvoicedAfterReOpened(),
            new SpunOffRepairOrder(),
            new ReOpened(),
            new AtDealership(),
            new TechAllocated(),
            new TechStoppedWork(),
            new TechRestartedWork(),
            new VehicleCompleted(),
            new CustomerContacted(),
            new Closed(),
            new Invoiced(),
        };

        public void AddPendingVolvoEventsToHistory(RepairOrderSnapshot repairOrder, VolvoEventHistory history)
        {
            foreach (var volvoEvent in PossibleVolvoEvents)
            {
                volvoEvent.Evaluate(repairOrder, history);
                if (volvoEvent.PreventsFurtherEvaluations)
                {
                    break;
                }
            }
        }
    }
}