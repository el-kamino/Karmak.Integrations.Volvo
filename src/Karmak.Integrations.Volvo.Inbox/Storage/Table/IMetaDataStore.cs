using Karmak.Integrations.Volvo.Inbox.Models;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public interface IMetaDataStore<T>
    where T : class, IMetaDataEntity, new()
{
    Task<T> CreateAsync(T entity);
    Task<List<T>> QueryAsync(string query);
    Task<T> GetByIdAsync(string accountNumber, string id);
    Task<T> UpdateAsync(T entity);
    Task<List<T>> BulkUpdateAsync(string accountNumber, List<T> entities);
}