using System.Linq.Expressions;
using Azure.Data.Tables;
using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IStorageService
    {
        #region Table Storage Methods

        Task<TableClient> GetStorageTableAsync(string? tableName);
        Task<T> GetEntityByRowKey<T>(string? tableName, string? rowKey) where T : BaseEntity, new();
        Task<T> GetEntityByConversationId<T>(string? tableName, string? conversationId) where T : BaseEntity, new();
        Task<IEnumerable<T>> GetEntitiesByQuery<T>(string? tableName, string? query, int maxPerPage = 100) where T : BaseEntity, new();
        Task<IEnumerable<T>> GetEntitiesByQuery<T>(string? tableName, Expression<Func<T, bool>> query) where T : BaseEntity, new();
        Task<T> InsertEntity<T>(string? tableName, T entity) where T : BaseEntity;
        Task<T> UpdateEntity<T>(string? tableName, T entity, TableUpdateMode updateMode = TableUpdateMode.Merge) where T : BaseEntity;
        Task<bool> RemoveEntity(string? tableName, string? rowKey);

        #endregion
    }
}
