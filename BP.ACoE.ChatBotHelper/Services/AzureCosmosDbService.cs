using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Microsoft.Azure.Cosmos;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class AzureCosmosDbService : ICosmosDbService
    {

        private readonly CosmosClient _dbClient;
        private readonly Container _container;


        public AzureCosmosDbService(CosmosClient dbClient,
            string databaseName,
            string containerName)
        {
            _dbClient = dbClient;
            this._container = dbClient.GetContainer(databaseName, containerName);
        }


        protected CosmosClient DbClient => _dbClient;

        public async Task<T> GetEntityById<T>(string id) where T : BaseCosmosEntity
        {
            try
            {
                var response = await this._container.ReadItemAsync<T>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new HttpRequestException($"Item {nameof(T)} with id: {id} not found");
            }
        }

        public async Task<IEnumerable<T>> GetEntitiesByQuery<T>(string queryString) where T : BaseCosmosEntity
        {
            var query = this._container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
            var results = new List<T>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();

                results.AddRange(response.ToList());
            }

            return results;
        }

        public async Task<T> InsertEntity<T>(T entity) where T : BaseCosmosEntity
        {
            var response = await this._container.CreateItemAsync<T>(entity, new PartitionKey(entity.Id));
            return response.Resource;
        }

        public async Task<T> UpdateEntity<T>(T entity) where T : BaseCosmosEntity
        {
            var response = await this._container.UpsertItemAsync<T>(entity, new PartitionKey(entity.Id));
            return response.Resource;
        }

        public async Task<T> RemoveEntity<T>(string id) where T : BaseCosmosEntity
        {
            var response = await this._container.DeleteItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
    }
}
