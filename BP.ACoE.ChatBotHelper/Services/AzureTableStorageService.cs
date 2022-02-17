using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class AzureTableStorageService : IStorageService
    {
        private readonly TableServiceClient _serviceClient;
        private readonly ILogger _logger;
        private readonly string _partitionKey;

        public AzureTableStorageService(TableServiceClient serviceClient, ILogger logger, string partitionKey)
        {
            _serviceClient = serviceClient;
            _logger = logger.ForContext<AzureTableStorageService>();
            _partitionKey = partitionKey;
        }


        public virtual  async Task<TableClient> GetStorageTableAsync(string tableName)
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

        public virtual async Task<BaseEntity> GetEntityByRowKey(string tableName, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            return await tableClient.GetEntityAsync<BaseEntity>(_partitionKey, rowKey);
        }

        public virtual async Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, string query, int maxPerPage = 100)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.QueryAsync<BaseEntity>(query, maxPerPage: maxPerPage);
            var pages = result.AsPages().GetAsyncEnumerator();
            var data = new List<BaseEntity>();
            do
            {
                data.AddRange(pages.Current.Values);
            } while (await pages.MoveNextAsync());
            return data;
        }

        public virtual async Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, Expression<Func<BaseEntity, bool>> query)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.QueryAsync(query);
            var pages = result.AsPages().GetAsyncEnumerator();
            var data = new List<BaseEntity>();
            do
            {
                data.AddRange(pages.Current.Values);
            } while (await pages.MoveNextAsync());
            return data;
        }

        public virtual async Task<BaseEntity> InsertEntity(string tableName, BaseEntity entity)
        {
            var table = _serviceClient.GetTableClient(tableName);
            var result = await table.AddEntityAsync(entity);
            if (!result.IsError) return result.Content.ToObjectFromJson<BaseEntity>();
            _logger.Error($"Invalid Entity {entity.ToJson()} create response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid create merge response, {result.ReasonPhrase}");
        }

        public virtual async Task<BaseEntity> UpdateEntity(string tableName, BaseEntity entity, TableUpdateMode updateMode = TableUpdateMode.Merge)
        {
            var table = _serviceClient.GetTableClient(tableName);
            var result = await table.UpdateEntityAsync(entity, ETag.All, updateMode);
            if (!result.IsError) return result.Content.ToObjectFromJson<BaseEntity>();
            _logger.Error($"Invalid Entity {entity.ToJson()} merge response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid Entity merge response, {result.ReasonPhrase}");
        }

        public virtual BaseEntity GetEntityByConversationId(string tableName, string conversationId)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.QueryAsync<BaseEntity>($"PartitionKey eq '{_partitionKey}' and ConversationId eq '{conversationId}'", maxPerPage: 1);
            var page = result.AsPages().GetAsyncEnumerator();
            var list = page.Current.Values;
            if (list.Any())
            {
                return list.First();
            }
            else
            {
                throw new HttpRequestException($"No entity was found for {conversationId}");
            }
        }

        public virtual async Task<bool> RemoveEntity(string tableName, string rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            var result = await tableClient.DeleteEntityAsync(_partitionKey, rowKey);
            if (!result.IsError) return true;
            _logger.Error($"Invalid Entity {rowKey} merge response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid Entity {rowKey} merge response, {result.ReasonPhrase}");

        }
    }
}
