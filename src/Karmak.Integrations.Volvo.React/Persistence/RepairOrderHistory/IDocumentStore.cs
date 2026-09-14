using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Persistence.RepairOrderHistory
{
    public interface IDocumentStore<T> where T : class, IPartitionedItem
    {
        Task<T> GetItemAsync(string id, string partitionKey);
        /// <summary>
        /// Update or insert an item
        /// </summary>
        /// <param name="item">Object to insert or update</param>
        /// <returns>THE EXACT SAME THING AS WE PUT IN</returns>
        Task<T> UpsertItemAsync(T item);
    }
}