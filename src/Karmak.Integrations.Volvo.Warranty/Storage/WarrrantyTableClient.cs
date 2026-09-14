using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Karmak.Integrations.Volvo.Common.Logging;
using Microsoft.Extensions.Logging;

namespace Karmak.Integrations.Volvo.Warranty.Storage
{
    public class WarrantyTableClient : IWarrantyTableClient
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<WarrantyTableClient> _logger;

        public WarrantyTableClient(TableServiceClient tableServiceClient, ILogger<WarrantyTableClient> logger)
        {
            _tableServiceClient = tableServiceClient ?? throw new ArgumentNullException(nameof(tableServiceClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CreateTableIfNotExists(string tableName, CancellationToken cancellationToken)
        {
            try
            {
                var tableClient = _tableServiceClient.GetTableClient(tableName);
                await tableClient.CreateIfNotExistsAsync(cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithMetadata(ex.Message, ex, new Dictionary<string, string> { { "TableName", tableName } });
                throw;
            }
        }

        public async Task CreateEntity<T>(string tableName, T entity) where T : class, ITableEntity, IWarrantyTableEntity
        {
            entity.PrepareToSave();
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.AddEntityAsync(entity);
        }

        public async Task UpdateEntity<T>(string tableName, T entity) where T : class, ITableEntity, IWarrantyTableEntity
        {
            entity.PrepareToSave();
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task<T> GetEntity<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken) where T : class, ITableEntity, IWarrantyTableEntity, new()
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            var response = await tableClient.GetEntityAsync<T>(partitionKey, rowKey, cancellationToken: cancellationToken);
            T result = response.Value;
            result.PopulateFollowingLoad();
            return result;
        }
    }
}
