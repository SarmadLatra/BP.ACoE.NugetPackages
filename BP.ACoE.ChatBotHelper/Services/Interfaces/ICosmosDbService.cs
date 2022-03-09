using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface ICosmosDbService
    {
        Task<T> GetEntityById<T>(string id) where T : BaseCosmosEntity;
        Task<IEnumerable<T>> GetEntitiesByQuery<T>(string queryString) where T : BaseCosmosEntity;
        Task<T> InsertEntity<T>(T entity) where T : BaseCosmosEntity;
        Task<T> UpdateEntity<T>(T entity) where T : BaseCosmosEntity;
        Task<T> RemoveEntity<T>(string id) where T : BaseCosmosEntity;
    }
}
