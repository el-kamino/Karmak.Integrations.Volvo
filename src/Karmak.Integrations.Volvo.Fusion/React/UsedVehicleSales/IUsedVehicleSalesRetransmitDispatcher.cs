namespace Karmak.Integrations.Volvo.Fusion.React.UsedVehicleSales
{
    public interface IUsedVehicleSalesRetransmitDispatcher
    {
        Task<Guid> RetransmitUsedVehicleSalesAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] invoiceNumbers);
    }
}