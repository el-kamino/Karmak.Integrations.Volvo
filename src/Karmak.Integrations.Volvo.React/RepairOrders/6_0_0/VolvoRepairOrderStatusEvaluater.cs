using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Volvo.Common.BlobClient;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0
{
    internal class VolvoRepairOrderStatusEvaluator : IVolvoRepairOrderStatusEvaluater
    {
        private const string _volvoVersion = "UDB6.0";
        private readonly ILogger _logger;
        private readonly IKarmakBlobClient _volvoReactBlobClient;

        public VolvoRepairOrderStatusEvaluator(ILogger<VolvoRepairOrderStatusEvaluator> logger, [FromKeyedServices("VolvoReact")] IKarmakBlobClient volvoReactBlobClient)
        {
            _logger = logger;
            _volvoReactBlobClient = volvoReactBlobClient;
        }

        /// <summary>
        /// Look for a blob named with the RepairOrderNumber and RepairOrderID. 
        /// If it exists, load it and return it. If not, create an initial state object based on the current snapshot. 
        /// </summary>
        /// <param name="repairOrder">Recieved RO snapshot</param>
        /// <returns>The previos state of the passed in RO</returns>
        public async Task<VolvoRepairOrderState> GetLastRoState(RepairOrderSnapshot repairOrder)
        {
            var roStateBlobPath = GetRoStateBlobPath(repairOrder.RepairOrderNumber, repairOrder.RepairOrderID.ToString());

            return await LastRoStateExists(roStateBlobPath) ? await LoadLastRoState(roStateBlobPath) : LoadInitialRoState(repairOrder);
        }

        /// <summary>
        /// Build up a state object for the RO based on the current snapshot, bringing values forward from last state as needed. 
        /// The most complex part of this is determining the current status of the RO, applying Volvo specific rules and hacks as needed.
        /// </summary>
        /// <param name="repairOrder">Recieved RO snapshot</param>
        /// <param name="lastRoState">Previous RO state</param>
        /// <returns>The current state of the passed in RO</returns>
        public VolvoRepairOrderState GetCurrentRoState(RepairOrderSnapshot repairOrder, VolvoRepairOrderState lastRoState)
        {
            var repairOrderStatus = GetVolvoRepairOrderStatus(repairOrder, lastRoState);

            return new VolvoRepairOrderState()
            {
                RepairOrderId = repairOrder.RepairOrderID,
                RepairOrderNumber = repairOrder.RepairOrderNumber,

                RepairOrderStatusState = repairOrderStatus,

                IsFirstTransmission = lastRoState.OriginalRoOpenDate == default,
                RoHasBeenClosed = lastRoState.RoHasBeenClosed || repairOrderStatus.VolvoRoStatus == VolvoRepairOrderStatus.CLOSED,
                OriginalRoOpenDate = lastRoState.OriginalRoOpenDate == default ? repairOrder.OpenDate.GetValueOrDefault() : lastRoState.OriginalRoOpenDate,
                HasSentVehicleArrival = lastRoState.HasSentVehicleArrival,
                HasSentTechnicianAllocated = lastRoState.HasSentTechnicianAllocated
            };
        }

        /// <summary>
        /// Builds up a status for the RO based on the header and task statuses, applying Volvo specific rules and hacks as needed. 
        /// The header status is determined with the following order of severity (highest to lowest): CANCELED, CLOSED, ON_HOLD, CONTACTED, COMPLETED, IN_PROGRESS, ARRIVED, RE_OPENED, UNDEFINED. 
        /// Task statuses are determined by their own status but are also impacted by the header status (e.g. if header is CLOSED, all tasks are considered COMPLETED or DECLINED). 
        /// Additionally, if an RO has ever been closed, any valid open status will be mapped to RE_OPENED until it is closed again.
        /// </summary>
        /// <param name="repairOrder">Recieved RO snapshot</param>
        /// <param name="roHasBeenClosed">Has the RO ever been closed</param>
        /// <returns>Status of the passed in RO</returns>
        public VolvoRepairOrderStatusState GetVolvoRepairOrderStatus(RepairOrderSnapshot repairOrder, VolvoRepairOrderState lastRoState)
        {
            string repairOrderStatus = GetHeaderStatusBasedOnRepairOrder(repairOrder, lastRoState);

            VolvoRepairOrderStatusState currentStatusState = new VolvoRepairOrderStatusState()
            { 
                VolvoRoStatus = repairOrderStatus,
                TaskStatusStates = repairOrder.Tasks.Select(t => new VolvoRepairOrderTaskStatusState
                {
                    TaskNumber = t.TaskNumber,
                    VolvoRoTaskStatus = GetTaskStatusBasedOnRepairTask(t, repairOrderStatus)
                }).ToList()
            };

            return currentStatusState;
        }

        /// <summary>
        /// Determine if this RO should trigger a send to Volvo based on changes in status from the last state to the current state.
        /// </summary>
        /// <param name="currentRoState">current RO state</param>
        /// <param name="lastRoState">Previous RO state</param>
        /// <returns>boolean</returns>
        public bool RepairOrderTriggersSend(VolvoRepairOrderState currentRoState, VolvoRepairOrderState lastRoState, bool forceTransmission)
        {
            var lastRoStatus = lastRoState.RepairOrderStatusState;

            if (currentRoState.RepairOrderStatusState.VolvoRoStatus == VolvoRepairOrderStatus.UNDEFINED)
            {
                //we do not recognize this status, do not send (even if forceTransmission)
                _logger.LogInformation(LogMessages.StatusUndefined, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion);
                return false;
            }

            if (currentRoState.IsFirstTransmission && currentRoState.RepairOrderStatusState.VolvoRoStatus == VolvoRepairOrderStatus.CANCELED)
            {
                //this is the first time we are seeing this RO, but it is a deleted RO, do not send (even if forceTransmission)
                _logger.LogInformation(LogMessages.UnseenCanceled, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion);
                return false;
            }

            if (currentRoState.IsFirstTransmission && currentRoState.RepairOrderStatusState.VolvoRoStatus != VolvoRepairOrderStatus.UNDEFINED)
            {
                //this is the first time we are seeing a valid status to send
                _logger.LogInformation(LogMessages.StatusNew, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion, currentRoState.RepairOrderStatusState.VolvoRoStatus);
                return true;
            }

            if (currentRoState.RepairOrderStatusState.VolvoRoStatus != lastRoStatus.VolvoRoStatus)
            {
                _logger.LogInformation(LogMessages.StatusRoChanged, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion, $"{lastRoStatus.VolvoRoStatus} to {currentRoState.RepairOrderStatusState.VolvoRoStatus}");
                return true;
            }

            if (HasDeletedOrChangedTasks(lastRoStatus, currentRoState)
                    || HasAddedTasks(lastRoStatus, currentRoState))
            {
                return true;
            }

            if (forceTransmission)
            {
                //did not trigger a state transition, but is marked to force transmission 
                _logger.LogInformation(LogMessages.ForceTransmission, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion);
                return true;
            }

            //did not trigger a state transition
            _logger.LogInformation(LogMessages.StatusNoChange, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion);
            return false;
        }

        /// <summary>
        /// Determine if we need to send a phantom payload of VEHICLE_ARRIVAL for this RO. 
        /// If this is the first send, and status is not ARRIVED we need to before any another state.
        /// </summary>
        /// <param name="currentRoState">current RO state</param>
        /// <returns>boolean</returns>
        public bool ShouldSendPhantomVehicleArrival(VolvoRepairOrderState currentRoState)
        {
            return !currentRoState.HasSentVehicleArrival
                && currentRoState.RepairOrderStatusState.VolvoRoStatus != VolvoRepairOrderStatus.ARRIVED;
        }

        /// <summary>
        /// Determine if we need to send a phantom payload of TECHNICIAN_ALLOCATED for this RO. 
        /// This is needed when we have not sent TECHNICIAN_ALLOCATED yet but the RO has reached any state except ARRIVED.
        /// </summary>
        /// <param name="currentRoState">current RO state</param>
        /// <returns>boolean</returns>
        public bool ShouldSendPhantomTechnicianAllocated(VolvoRepairOrderState currentRoState)
        {
            return !currentRoState.HasSentTechnicianAllocated 
                && currentRoState.RepairOrderStatusState.VolvoRoStatus != VolvoRepairOrderStatus.ARRIVED;
        }


        /// <summary>
        /// Determine if we need to send a phantom payload of VEHICLE_ARRIVAL and TECHNICIAN_ALLOCATED for this RO Retransmit. 
        /// If this was the first send, and status is not ARRIVED we need to before any another state.
        /// </summary>
        /// <param name="lastRoState">last RO state</param>
        /// <returns>boolean</returns>
        public bool ShouldSendPhantomPayloadsDuringRetransmit(VolvoRepairOrderState lastRoState)
        {
            return lastRoState.IsFirstTransmission
                && lastRoState.RepairOrderStatusState.VolvoRoStatus != VolvoRepairOrderStatus.ARRIVED;
        }

        private VolvoRepairOrderState LoadInitialRoState(RepairOrderSnapshot repairOrder)
        {
            return new VolvoRepairOrderState()
            {
                RepairOrderId = repairOrder.RepairOrderID,
                RepairOrderNumber = repairOrder.RepairOrderNumber,
                RepairOrderStatusState = new VolvoRepairOrderStatusState() { VolvoRoStatus = VolvoRepairOrderStatus.UNDEFINED }
            };
        }

        private bool HasDeletedOrChangedTasks(VolvoRepairOrderStatusState lastRoStatus, VolvoRepairOrderState currentRoState)
        {
            var deletedOrChangedTask = lastRoStatus.TaskStatusStates
                .Select(lastTaskState => new
                {
                    LastTask = lastTaskState,
                    CurrentTask = currentRoState.RepairOrderStatusState.TaskStatusStates
                        .FirstOrDefault(t => t.TaskNumber == lastTaskState.TaskNumber)
                })
                .FirstOrDefault(pair => pair.CurrentTask == null ||
                                       pair.CurrentTask.VolvoRoTaskStatus != pair.LastTask.VolvoRoTaskStatus);

            if (deletedOrChangedTask != null)
            {
                if (deletedOrChangedTask.CurrentTask == null)
                {
                    _logger.LogInformation(LogMessages.StatusTaskChanged, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion, $"Deleted Task {deletedOrChangedTask.LastTask.TaskNumber}.");
                }
                else
                {
                    _logger.LogInformation(LogMessages.StatusTaskChanged, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion, $"Task {deletedOrChangedTask.LastTask.TaskNumber} from {deletedOrChangedTask.LastTask.VolvoRoTaskStatus} to {deletedOrChangedTask.CurrentTask.VolvoRoTaskStatus}");
                }
                return true;
            }

            return false;
        }

        private bool HasAddedTasks(VolvoRepairOrderStatusState lastRoStatus, VolvoRepairOrderState currentRoState)
        {
            var addedTask = currentRoState.RepairOrderStatusState.TaskStatusStates
                .FirstOrDefault(currentTaskState => !lastRoStatus.TaskStatusStates
                    .Any(t => t.TaskNumber == currentTaskState.TaskNumber));
            if (addedTask != null)
            {
                _logger.LogInformation(LogMessages.StatusTaskChanged, $"Repair Order {currentRoState.RepairOrderNumber}", _volvoVersion, $"Added Task {addedTask.TaskNumber}.");
                return true;
            }

            return false;
        }

        private static string GetTaskStatusBasedOnRepairTask(RepairOrderTask task, string repairOrderStatus)
        {
            //Volvo hack: if the RO is CLOSED, all tasks must be COMPLETED or DECLINED
            if (repairOrderStatus == VolvoRepairOrderStatus.CLOSED)
            {
                if (task.RepairTaskStatus.ToLower() == RepairOrderTaskStatus.QUOTE_DECLINED)
                    return VolvoRepairOrderTaskStatus.DECLINED;

                return VolvoRepairOrderTaskStatus.COMPLETED;
            }

            return task.RepairTaskStatus.ToLower() switch
            {
                RepairOrderTaskStatus.HOLD => VolvoRepairOrderTaskStatus.ON_HOLD,
                RepairOrderTaskStatus.WAITING_FOR_PARTS => VolvoRepairOrderTaskStatus.ON_HOLD,
                RepairOrderTaskStatus.CLOSED => VolvoRepairOrderTaskStatus.COMPLETED,
                RepairOrderTaskStatus.QUOTE_DECLINED => VolvoRepairOrderTaskStatus.DECLINED,
                _ => TaskHasLaborEntries(task) ? VolvoRepairOrderTaskStatus.IN_PROGRESS : VolvoRepairOrderTaskStatus.WORK_NOT_STARTED,
            };
        }

        private static string GetHeaderStatusBasedOnRepairOrder(RepairOrderSnapshot repairOrder, VolvoRepairOrderState lastRoState)
        {
            //The order of these checks is **important**.
            //  1) CANCELED, CLOSED - a final status. No reason to continue.
            //  2) else ON_HOLD,COMPLETED - apply to all tasks. No reason to continue. If COMPLETED, also check substatus READY_FOR_PICKUP.
            //  3) else IN_PROGRESS - has any labor.
            //  4) else check ARRIVED - which we can only send once.
            //  5) else UNDEFINED - no Volvo state.
            //** Volvo hack - once an RO has been CLOSED, if it goes back to any valid open state, we ALWAYS send RE_OPENED, until it closes again.

            if (repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.VOIDED))
                return lastRoState.RoHasBeenClosed ? VolvoRepairOrderStatus.UNDEFINED : VolvoRepairOrderStatus.CANCELED; //Can not cancel an RO that has been closed

            if (repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.CLOSED) ||
                repairOrder.RepairOrderStatus.EqualsIgnoreCase(RepairOrderStatus.INVOICED))
                return VolvoRepairOrderStatus.CLOSED;

            if (RoAllTasksAreOnHold(repairOrder))
                return SwapRE_OPENEDIfNeeded(VolvoRepairOrderStatus.ON_HOLD, lastRoState.RoHasBeenClosed);

            if (RoAllTasksAreComplete(repairOrder))
                return repairOrder.CustomerContactedStatus.EqualsIgnoreCase(RepairOrderSubstatus.READY_FOR_PICKUP)
                    ? SwapRE_OPENEDIfNeeded(VolvoRepairOrderStatus.CONTACTED, lastRoState.RoHasBeenClosed)
                    : SwapRE_OPENEDIfNeeded(VolvoRepairOrderStatus.COMPLETED, lastRoState.RoHasBeenClosed);

            if (RoHasLaborEntries(repairOrder))
                return SwapRE_OPENEDIfNeeded(VolvoRepairOrderStatus.IN_PROGRESS, lastRoState.RoHasBeenClosed);

            if (repairOrder.SubStatus.EqualsIgnoreCase(RepairOrderSubstatus.AT_DEALERSHIP) && !lastRoState.HasSentVehicleArrival)
                return SwapRE_OPENEDIfNeeded(VolvoRepairOrderStatus.ARRIVED, lastRoState.RoHasBeenClosed);

            return VolvoRepairOrderStatus.UNDEFINED;
        }

        private static string SwapRE_OPENEDIfNeeded(string currentStatus, bool roHasBeenClosed)
        {
            return roHasBeenClosed ? VolvoRepairOrderStatus.RE_OPENED : currentStatus;
        }

        private static bool TaskHasLaborEntries(RepairOrderTask task)
        {
            return task.LaborEntries.Any();
        }

        private static bool RoHasLaborEntries(RepairOrderSnapshot repairOrder)
        {
            return repairOrder.Tasks.Any(t => t.LaborEntries.Any());
        }

        private static bool RoAllTasksAreComplete(RepairOrderSnapshot repairOrder)
        {
            bool hasOpenTasks = repairOrder.Tasks
                .Any(t => !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.CLOSED) &&
                          !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.QUOTE_DECLINED));

            return repairOrder.Tasks.Any() && !hasOpenTasks;
        }

        private static bool RoAllTasksAreOnHold(RepairOrderSnapshot repairOrder)
        {
            var openTasks = repairOrder.Tasks
                .Where(t => !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.CLOSED) &&
                            !t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.QUOTE_DECLINED))
                .ToList();

            return openTasks.Any() && openTasks
                .All(t => t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.HOLD) ||
                          t.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.WAITING_FOR_PARTS));
        }

        #region blob state storage
        private string GetRoStateBlobPath(string repairOrderNumber, string repairOrderID)
        {
            return $"{ImplicitElkContext.Current.ApplicationContext.Company.ToString()}/RepairOrderStateStore/{ImplicitElkContext.Current.ApplicationContext.Branch.ToString()}/RepairOrderNumber_{repairOrderNumber}_ID_{repairOrderID}_State.json";
        }

        private async Task<bool> LastRoStateExists(string path)
        {
            return await _volvoReactBlobClient.BlobExistsAsync(path);
        }

        private async Task<VolvoRepairOrderState> LoadLastRoState(string path)
        {
            return await _volvoReactBlobClient.RetrieveAsync<VolvoRepairOrderState>(path);
        }

        public async Task SaveRepairOrderState(VolvoRepairOrderState currentRepairOrderState)
        {
            await _volvoReactBlobClient.UploadAsync(GetRoStateBlobPath(currentRepairOrderState.RepairOrderNumber, currentRepairOrderState.RepairOrderId.ToString()), currentRepairOrderState);
        }
        #endregion
    }
}
