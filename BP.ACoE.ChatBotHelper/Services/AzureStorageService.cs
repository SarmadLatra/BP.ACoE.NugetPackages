using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Microsoft.VisualBasic;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class AzureStorageService : IStorageService
    {
        private readonly TableServiceClient _serviceClient;
        private readonly ILogger _logger;
        private readonly string _partitionKey;

        public AzureStorageService(TableServiceClient serviceClient, ILogger logger, string partitionKey)
        {
            _serviceClient = serviceClient;
            _logger = logger.ForContext<AzureStorageService>();
            _partitionKey = partitionKey;
        }


        public async Task<TableClient> GetStorageTableAsync(string tableName)
        {

            var tableClient = _serviceClient.GetTableClient(tableName);
            if (tableClient is not null)
            {
                return tableClient;
            }
            else
            {
                var result = await _serviceClient.CreateTableIfNotExistsAsync(tableName);
                _logger.Information($"azure table created for {result.Value.Name}");
                return _serviceClient.GetTableClient(tableName);
            }
        }

        public async Task<BaseEntity> GetEntityByRowKey(string tableName, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            return await tableClient.GetEntityAsync<BaseEntity>(partitionKey: _partitionKey, rowKey);
        }

        public async Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, string query, int maxPerPage = 100)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.QueryAsync<BaseEntity>(query, maxPerPage: maxPerPage);
            var pages = result.AsPages();
            return pages.GetAsyncEnumerator().Current.Values;
        }

        public Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, string query)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncPageable<ITableEntity>> GetEntitiesByQuery(string tableName, string query, string continuationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AsyncPageable<ITableEntity>> GetEntitiesByQuery(string tableName, Expression query, string continuationToken)
        {
            throw new NotImplementedException();
        }

        public Task<BaseEntity> InsertEntity(string tableName, BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<BaseEntity> UpdateEntity(string tableName, BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public BaseEntity GetEntityByConversationId(string tableName, string conversationId)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.QueryAsync<BaseEntity>($"PartitionKey eq '{_partitionKey}' and ConversationId eq '{conversationId}'", maxPerPage: 1);
            var page = result.AsPages();
            var list = page.GetAsyncEnumerator().Current.Values;
            if (list.Any())
            {
                return list.First();
            }
            else
            {
                throw new HttpRequestException($"No entity was found for {conversationId}");
            }
        }

        public async Task<bool> RemoveEntity(string tableName, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            var result = await tableClient.DeleteEntityAsync(_partitionKey, rowKey);
            if (result.IsError)
            {
                throw new HttpRequestException(result.ReasonPhrase);
            }

            return true;
        }
    }
}
