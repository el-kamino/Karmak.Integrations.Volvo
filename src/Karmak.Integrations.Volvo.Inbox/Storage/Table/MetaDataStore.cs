using Azure.Data.Tables;
using Karmak.Integrations.Volvo.Inbox.Models;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table
{
    public class MetaDataStore<T> : IMetaDataStore<T>
        where T : class, IMetaDataEntity, new()
    {
        private readonly TableServiceClient _tableClient;
        private readonly MetaDataStoreOptions _options;

        public MetaDataStore(IOptions<MetaDataStoreOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options.Value));
            _tableClient = new TableServiceClient(_options.TableConnectionString);

            _tableClient.CreateTableIfNotExistsAsync(_options.TableName).GetAwaiter().GetResult();
        }

        public async Task<T> CreateAsync(T entity)
        {
            var table = _tableClient.GetTableClient(_options.TableName);

            await table.AddEntityAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var result = await _tableClient
                .GetTableClient(_options.TableName)
                .UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);

            return entity;
        }

        public async Task<T> GetByIdAsync(string accountNumber, string id)
        {
            return await _tableClient
                .GetTableClient(_options.TableName)
                .GetEntityAsync<T>(accountNumber, id);
        }

        public async Task<List<T>> QueryAsync(string query)
        {
            var results = await _tableClient
                  .GetTableClient(_options.TableName)
                  .QueryAsync<T>(query).ToListAsync();

            return results;
        }

        public async Task<List<T>> BulkUpdateAsync(string accountNumber, List<T> entities)
        {
            var batch = new List<TableTransactionAction>();

            foreach (var entity in entities)
            {
                batch.Add(new TableTransactionAction(TableTransactionActionType.UpdateReplace, entity));
            }

            await _tableClient.GetTableClient(_options.TableName).SubmitTransactionAsync(batch);
            return entities;
        }
    }        
}