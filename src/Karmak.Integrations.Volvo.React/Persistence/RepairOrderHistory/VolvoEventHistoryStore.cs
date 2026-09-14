using System.Collections.Generic;
using System.Threading.Tasks;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents;

namespace Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory
{
    public class VolvoEventHistoryStore : IVolvoEventHistoryProvider
    {
        private readonly IDocumentStore<VolvoEventHistory> _documentStore;

        public VolvoEventHistoryStore(IDocumentStore<VolvoEventHistory> documentStore)
        {
            _documentStore = documentStore;
        }

        public async Task<VolvoEventHistory> GetVolvoEventHistory(DealerInfo dealerInfo, string repairOrderNumber)
        {
            string id = $"{dealerInfo.BranchId}_{repairOrderNumber}";
            string partitionKey = $"{dealerInfo.AccountId}/{dealerInfo.InstanceId}";
            var history = await _documentStore.GetItemAsync(id, partitionKey);
            return history ?? new VolvoEventHistory
            {
                Id = id,
                PartitionKey = partitionKey,
                RepairOrderNumber = repairOrderNumber,
                Events = new List<VolvoEvent>()
            };
        }

        /// <summary>
        /// Update or insert an event history item
        /// </summary>
        /// <param name="log">Event history item to insert or update</param>
        /// <returns>A COPY OF THE EXACT SAME ITEM AS WE SAVE, INCLUDING THE SAME ETAG</returns>
        public Task<VolvoEventHistory> UpsertVolvoEventHistory(VolvoEventHistory log)
        {
            return _documentStore.UpsertItemAsync(log);
        }
    }
}