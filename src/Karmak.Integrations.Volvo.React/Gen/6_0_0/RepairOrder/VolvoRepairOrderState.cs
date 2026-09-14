using System;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0
{
    public class VolvoRepairOrderState
    {
        public VolvoRepairOrderState()
        {
            RepairOrderStatusState = new VolvoRepairOrderStatusState();
        }

        public int RepairOrderId { get; set; }
        public string RepairOrderNumber { get; set; }
        public bool IsFirstTransmission { get; set; } = false;
        public bool RoHasBeenClosed { get; set; } = false;
        public string CommentHash { get; set; }
        public bool HasSentVehicleArrival { get; set; } = false;
        public bool HasSentTechnicianAllocated { get; set; } = false;
        public DateTime OriginalRoOpenDate { get; set; }
        public VolvoRepairOrderStatusState RepairOrderStatusState { get; set; }
    }

    public class VolvoRepairOrderStatusState
    {
        public string VolvoRoStatus { get; set; }
        public List<VolvoRepairOrderTaskStatusState> TaskStatusStates { get; set; }

        public VolvoRepairOrderStatusState()
        {
            TaskStatusStates = new List<VolvoRepairOrderTaskStatusState>();
        }
    }

    public class VolvoRepairOrderTaskStatusState
    {
        public int? TaskNumber { get; set; }
        public string VolvoRoTaskStatus { get; set; }
    }
}
