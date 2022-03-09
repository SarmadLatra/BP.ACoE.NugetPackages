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
        private readonly string? _partitionKey;

        public AzureTableStorageService(TableServiceClient serviceClient, ILogger logger, string? partitionKey)
        {
            _serviceClient = serviceClient;
            _logger = logger.ForContext<AzureTableStorageService>();
            _partitionKey = partitionKey;
        }


        public virtual async Task<TableClient> GetStorageTableAsync(string? tableName)
        {

            var tableClient = _serviceClient.GetTableClient(tableName);
            if (tableClient is not null)
            {
                return tableClient;
            }
            else
            {
                var result = await _serviceClient.CreateTableIfNotExistsAsync(tableName);
                _logger.Information($"azure table created for {result?.Value?.Name}");
                return _serviceClient.GetTableClient(tableName);
            }
        }

        public virtual async Task<T> GetEntityByRowKey<T>(string? tableName, string? rowKey) where T : BaseEntity, new()
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            return await tableClient.GetEntityAsync<T>(_partitionKey, rowKey);
        }

        public virtual async Task<IEnumerable<T>> GetEntitiesByQuery<T>(string? tableName, string? query, int maxPerPage = 100) where T : BaseEntity, new()
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.Query<T>(query);
            await Task.Delay(0);
            return result?.ToList() ?? new List<T>();
        }

        public virtual async Task<IEnumerable<T>> GetEntitiesByQuery<T>(string? tableName, Expression<Func<T, bool>> query) where T : BaseEntity, new()
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.Query(query);
            await Task.Delay(0);
            return result?.ToList() ?? new List<T>();
        }

        public virtual async Task<T> InsertEntity<T>(string? tableName, T entity) where T : BaseEntity
        {
            var table = _serviceClient.GetTableClient(tableName);
            var result = await table.AddEntityAsync(entity);
            if (!result.IsError) return entity;
            _logger.Error($"Invalid Entity {entity.ToJson()} create response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid create merge response, {result.ReasonPhrase}");
        }

        public virtual async Task<T> UpdateEntity<T>(string? tableName, T entity, TableUpdateMode updateMode = TableUpdateMode.Merge) where T : BaseEntity
        {
            var table = _serviceClient.GetTableClient(tableName);
            var result = await table.UpdateEntityAsync(entity, ETag.All, updateMode);
            if (!result.IsError) return entity;
            _logger.Error($"Invalid Entity {entity.ToJson()} merge response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid Entity merge response, {result.ReasonPhrase}");
        }

        public virtual async Task<T> GetEntityByConversationId<T>(string? tableName, string? conversationId) where T : BaseEntity, new()
        {
            var tableClient = _serviceClient.GetTableClient(tableName);
            var result = tableClient.Query<T>($"PartitionKey eq '{_partitionKey}' and ConversationId eq '{conversationId}'", maxPerPage: 1);
            await Task.Delay(0);
            var list = result?.ToList() ?? new List<T>();
            if (list.Any())
            {
                return list.First();
            }
            else
            {
                throw new HttpRequestException($"No entity was found for {conversationId}");
            }
        }

        public virtual async Task<bool> RemoveEntity(string? tableName, string? rowKey)
        {
            var tableClient = _serviceClient.GetTableClient(tableName);

            var result = await tableClient.DeleteEntityAsync(_partitionKey, rowKey);
            if (!result.IsError) return true;
            _logger.Error($"Invalid Entity {rowKey} merge response, {result.ReasonPhrase}");
            throw new HttpRequestException($"Invalid Entity {rowKey} merge response, {result.ReasonPhrase}");

        }
    }
}
