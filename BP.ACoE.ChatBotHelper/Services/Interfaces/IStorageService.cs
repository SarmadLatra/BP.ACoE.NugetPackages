using System.Linq.Expressions;
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
        Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, string query, int maxPerPage = 100);
        Task<IEnumerable<BaseEntity>> GetEntitiesByQuery(string tableName, Expression<Func<BaseEntity, bool>> query);
        Task<BaseEntity> InsertEntity(string tableName, BaseEntity entity);
        Task<BaseEntity> UpdateEntity(string tableName, BaseEntity entity, TableUpdateMode updateMode = TableUpdateMode.Merge);
        Task<bool> RemoveEntity(string tableName, string rowKey);

        #endregion
    }
}
