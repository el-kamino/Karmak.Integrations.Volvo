using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0
{
    public interface IVolvoRepairOrderStatusEvaluater
    {
        Task<VolvoRepairOrderState> GetLastRoState(RepairOrderSnapshot repairOrder);
        VolvoRepairOrderState GetCurrentRoState(RepairOrderSnapshot repairOrder, VolvoRepairOrderState lastRoSate);
        VolvoRepairOrderStatusState GetVolvoRepairOrderStatus(RepairOrderSnapshot repairOrder, VolvoRepairOrderState lastRoState);
        bool RepairOrderTriggersSend(VolvoRepairOrderState currentRoState, VolvoRepairOrderState lastRoState, bool forceTransmission);
        bool ShouldSendPhantomVehicleArrival(VolvoRepairOrderState currentRoState);
        bool ShouldSendPhantomTechnicianAllocated(VolvoRepairOrderState currentRoState);
        bool ShouldSendPhantomPayloadsDuringRetransmit(VolvoRepairOrderState lastRoState);
        Task SaveRepairOrderState(VolvoRepairOrderState currentRepairOrderState);
    }
}

