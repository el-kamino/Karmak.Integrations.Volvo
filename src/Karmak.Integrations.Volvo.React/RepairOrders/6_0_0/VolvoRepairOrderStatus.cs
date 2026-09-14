namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0
{
    internal static class VolvoRepairOrderStatus
    {
        public const string ARRIVED = "VEHICLE_ARRIVAL"; //Phantom. Must be the first status sent.  Do not send Tasks
        public const string TECH_ALLOCATED = "TECHNICIAN_ALLOCATED"; //Phantom. Do not have this status, send before all other valid statuses except VEHICLE_ARRIVAL.  All Tasks = WORK_NOT_STARTED
        public const string IN_PROGRESS = "REPAIR_WORK_IN_PROGRESS"; //At least one task REPAIR_WORK_IN_PROGRESS
        public const string ON_HOLD = "REPAIR_WORK_ON_HOLD"; //All tasks LINE_ON_HOLD, WORK_COMPLETED or DECLINED
        public const string COMPLETED = "VEHICLE_COMPLETED"; //All tasks WORK_COMPLETED or DECLINED
        public const string CONTACTED = "CUSTOMER_CONTACTED";  //All tasks WORK_COMPLETED or DECLINED
        public const string DELIVERED = "VEHICLE_TO_CUSTOMER";  //Not used currently
        public const string CLOSED = "CLOSED";  //RO is invoiced or closed. All tasks WORK_COMPLETED or DECLINED
        public const string RE_OPENED = "RE_OPENED"; //stay in this state indefinitely until closed again.
        public const string CANCELED = "CANCELED";  //Can not send if RO has been closed.  RO is deleted in Fusion.

        public const string UNDEFINED = "UNDEFINED";  //No status that matches
    }

    internal static class VolvoRepairOrderTaskStatus
    {
        public const string WORK_NOT_STARTED = "WORK_NOT_STARTED";  //no time logged against the task
        public const string IN_PROGRESS = "REPAIR_WORK_IN_PROGRESS";  //time logged against the task
        public const string ON_HOLD = "LINE_ON_HOLD";  //Task status is "Hold" or "Waiting for Parts".
        public const string COMPLETED = "WORK_COMPLETED"; //Task status is "Closed"
        public const string PRE_APPROVAL = "PRE_APPROVAL";  //Not used currently
        public const string WARRANTY_CLAIM_SUBMITTED = "WARRANTY_CLAIM_SUBMITTED"; //Not used currently
        public const string DECLINED = "DECLINED";  //Task status is "Declined"
    }
}
