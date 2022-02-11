using System.Linq.Expressions;
using Azure;
using Azure.Data.Tables;
using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IStorageService
    {

        #region Table Storage Methods

        Task<TableClient> GetStorageTableAsync(string tableName);

        Task<BaseEntity> GetEntityByRowKey(string tableName, string rowKey);
        BaseEntity GetEntityByConversationId(string tableName, string conversationId);
        Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, string query);
        Task<AsyncPageable<ITableEntity>> GetEntitiesByQuery(string tableName, string query, string continuationToken);
        Task<AsyncPageable<ITableEntity>> GetEntitiesByQuery(string tableName, Expression query, string continuationToken);
        Task<BaseEntity> InsertEntity(string tableName, BaseEntity entity);
        Task<BaseEntity> UpdateEntity(string tableName, BaseEntity entity);
        Task<bool> RemoveEntity(string tableName, string rowKey);

        #endregion
    }
}
