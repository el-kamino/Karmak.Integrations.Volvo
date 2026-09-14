namespace Karmak.Integrations.Volvo.Fusion.React.PartsSalesOrders;

public interface IPartsSalesOrderRetransmitDispatcher
{
    Task<Guid> RetransmitPartsSalesOrderAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] invoiceNumbers);
}