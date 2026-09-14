using System.Threading.Tasks;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents;

namespace Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory
{
    public interface IVolvoEventHistoryProvider
    {
        Task<VolvoEventHistory> GetVolvoEventHistory(DealerInfo dealerInfo, string repairOrderNumber);


        /// <summary>
        /// Update or insert an event history item
        /// </summary>
        /// <param name="log">Event history item to insert or update</param>
        /// <returns>A COPY OF THE EXACT SAME ITEM AS WE SAVE, INCLUDING THE SAME ETAG</returns>
        Task<VolvoEventHistory> UpsertVolvoEventHistory(VolvoEventHistory log);
    }
}