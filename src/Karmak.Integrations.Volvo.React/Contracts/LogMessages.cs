namespace Karmak.Integrations.Volvo.React.Contracts
{
    public static class LogMessages
    {
        //Information messages
        public const string VersionNotEnabled = "{EntityIdentifier} not processed for this version per configuration. {VolvoVersion}";
        public const string ReceivedPayload = "{EntityIdentifier} received from Service Bus. {VolvoVersion}";
        public const string RetransmitReceivedPayload = "{EntityIdentifier} retransmit received from Service Bus. {VolvoVersion}";
        public const string FinishedProcessing = "{EntityIdentifier} finished processing. {VolvoVersion}";
        public const string ForceTransmission = "{EntityIdentifier} is marked to force transmission. {VolvoVersion}";

        //Error messages
        public const string FailedValidation = "{EntityIdentifier} failed validation. {VolvoVersion} Error: {ErrorMessage}";
        public const string InvalidSettings = "{EntityIdentifier} invalid settings found for Volvo React. {VolvoVersion} Error: {ErrorMessage}";
        public const string SnapshotOutOfOrder = "{EntityIdentifier} snapshot is out of order. {VolvoVersion} Error: {ErrorMessage}";
        public const string CommentsTransmitException = "{EntityIdentifier} transmit comments exception. {VolvoVersion} Error: {ErrorMessage}";

        //RO Specific log messages
        public const string SendingVehicleArrival = "{EntityIdentifier} has not sent VEHICLE_ARRIVAL, sending initial payload. {VolvoVersion}";
        public const string SendingTechAllocated = "{EntityIdentifier} has not sent TECHNICIAN_ALLOCATED, sending initial payload. {VolvoVersion}";
        public const string RoProcessing = "{EntityIdentifier} Repair Order processing for P&A Code {PaCode}. {VolvoVersion}";
        public const string RoRetransmitting = "{EntityIdentifier} Repair Order retransmitting for P&A Code {PaCode}. {VolvoVersion}";
        public const string NoCommentChanges = "{EntityIdentifier} no changes to comments. {VolvoVersion}";
        public const string StatusUndefined = "{EntityIdentifier} did not have a valid Volvo status. {VolvoVersion}";
        public const string UnseenCanceled = "{EntityIdentifier} was deleted, but has not been sent to Volvo before. {VolvoVersion}";
        public const string StatusNew = "{EntityIdentifier} triggered first transmission. {VolvoVersion} Trigger: {Trigger}";
        public const string StatusRoChanged = "{EntityIdentifier} triggered RO Status change. {VolvoVersion} Trigger: {Trigger}";
        public const string StatusTaskChanged = "{EntityIdentifier} triggered RO Task Status change. {VolvoVersion} Trigger: {Trigger}";
        public const string StatusNoChange = "{EntityIdentifier} did not trigger a state transition. {VolvoVersion}";
        public const string NoTasks = "{EntityIdentifier} had no valid Tasks. {VolvoVersion}";
        public const string CommentsTransmitted = "{EntityIdentifier} comments transmitted. {VolvoVersion}";
        public const string CommentsNotTransmitted = "{EntityIdentifier} comments not transmitted, see prior telemetry. {VolvoVersion}";
    }
}
