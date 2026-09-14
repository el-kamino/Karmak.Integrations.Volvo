namespace Karmak.Integrations.Volvo.Fusion.React.RepairOrders
{
    public interface IRepairOrderRetransmitDispatcher
    {
        Task<Guid> RetransmitRepairOrdersAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] repairOrderNumbers);
    }
}