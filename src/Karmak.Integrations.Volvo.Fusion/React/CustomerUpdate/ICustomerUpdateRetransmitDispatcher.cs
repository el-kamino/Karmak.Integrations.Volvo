namespace Karmak.Integrations.Volvo.Fusion.React.CustomerUpdate
{
    public interface ICustomerUpdateRetransmitDispatcher
    {
        Task<Guid> RetransmitCustomerUpdateAsync(string paCode, DateTime? beginDateTimeWindow, DateTime? endDateTimeWindow, string[] volvoPassIds, string[] vins);
    }
}