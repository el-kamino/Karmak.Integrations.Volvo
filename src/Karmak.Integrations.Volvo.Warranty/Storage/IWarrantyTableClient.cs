using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Karmak.Integrations.Volvo.Warranty.Storage
{
    public interface IWarrantyTableClient
    {
        Task<bool> CreateTableIfNotExists(string tableName, CancellationToken cancellationToken);
        Task CreateEntity<T>(string tableName, T entity) where T : class, ITableEntity, IWarrantyTableEntity;
        Task UpdateEntity<T>(string tableName, T entity) where T : class, ITableEntity, IWarrantyTableEntity;
        Task<TEntity> GetEntity<TEntity>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default)
            where TEntity : class, ITableEntity, IWarrantyTableEntity, new();
    }
}
