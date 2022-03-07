using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class AzureCosmosDbServiceUnitTest
    {
        [Fact]
        public async Task TestGetEntityByIdAzureTableStorageServiceAsync()
        {
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockContainer = new Mock<Container>();

            var mockItemResponse = new Mock<ItemResponse<BaseCosmosEntity>>();
            mockItemResponse.Setup(x => x.StatusCode).Returns(It.IsAny<HttpStatusCode>);
            mockItemResponse.Setup(x => x.Resource).Returns(new MockBaseCosmosEntity() { Id = "mockId", Name = "mockName" });

            mockContainer.Setup(c => c.ReadItemAsync<BaseCosmosEntity>
         (It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(),
         It.IsAny<CancellationToken>())).ReturnsAsync(mockItemResponse.Object);

            mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(),
                It.IsAny<string>())).Returns(mockContainer.Object);

            var obj = new AzureCosmosDbService(mockCosmosClient.Object, "mockDatabase", "mockContainer");

            var getEntityById = await obj.GetEntityById<BaseCosmosEntity>("mockId");

            Assert.Equal("BaseEntity", getEntityById.Type);
        }

        [Fact]
        public async Task TestGetEntityByQueryAzureTableStorageServiceAsync()
        {
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockContainer = new Mock<Container>();

            var myItems = new List<BaseCosmosEntity>() { new MockBaseCosmosEntity() { }, new MockBaseCosmosEntity() { } };

            var feedResponseMock = new Mock<FeedResponse<BaseCosmosEntity>>();
            feedResponseMock.Setup(x => x.GetEnumerator()).Returns(myItems.GetEnumerator());

            feedResponseMock.Setup(x => x.Resource)
                .Returns(feedResponseMock.Object);

            var feedIteratorMock = new Mock<FeedIterator<BaseCosmosEntity>>();
            feedIteratorMock.Setup(f => f.HasMoreResults).Returns(true);
            feedIteratorMock
                .Setup(f => f.ReadNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(feedResponseMock.Object)
                .Callback(() => feedIteratorMock
                    .Setup(f => f.HasMoreResults)
                    .Returns(false));

            mockContainer
                .Setup(c => c.GetItemQueryIterator<BaseCosmosEntity>(
                    It.IsAny<QueryDefinition>(),
                    It.IsAny<string>(),
                    It.IsAny<QueryRequestOptions>()))
                .Returns(feedIteratorMock.Object);

            mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(),
                It.IsAny<string>())).Returns(mockContainer.Object);

            var obj = new AzureCosmosDbService(mockCosmosClient.Object, "mockDatabase", "mockContainer");

            var getEntitesByQuery = await obj.GetEntitiesByQuery<BaseCosmosEntity>("mockQueryString");

            Assert.Equal("BaseEntity", getEntitesByQuery?.FirstOrDefault()?.Type);
        }

        [Fact]
        public async Task TestCRUDAzureTableStorageServiceAsync()
        {
            var mockCosmosClient = new Mock<CosmosClient>();
            var mockContainer = new Mock<Container>();

            var mockItemResponse = new Mock<ItemResponse<BaseCosmosEntity>>();
            mockItemResponse.Setup(x => x.StatusCode).Returns(It.IsAny<HttpStatusCode>);
            mockItemResponse.Setup(x => x.Resource).Returns(new MockBaseCosmosEntity()
            { Id = "mockId", Name = "mockName" });

            mockContainer.Setup(c => c.CreateItemAsync<BaseCosmosEntity>
         (It.IsAny<BaseCosmosEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(),
         It.IsAny<CancellationToken>())).ReturnsAsync(mockItemResponse.Object);

            mockContainer.Setup(c => c.UpsertItemAsync<BaseCosmosEntity>
        (It.IsAny<BaseCosmosEntity>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(),
        It.IsAny<CancellationToken>())).ReturnsAsync(mockItemResponse.Object);

            mockContainer.Setup(c => c.DeleteItemAsync<BaseCosmosEntity>
       (It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(),
       It.IsAny<CancellationToken>())).ReturnsAsync(mockItemResponse.Object);

            mockCosmosClient.Setup(x => x.GetContainer(It.IsAny<string>(),
                It.IsAny<string>())).Returns(mockContainer.Object);

            var obj = new AzureCosmosDbService(mockCosmosClient.Object, "mockDatabase", "mockContainer");

            var insertEnity = await obj.InsertEntity<BaseCosmosEntity>(new MockBaseCosmosEntity()
            {
                Id = "mockId",
                Name = "mockName"
            });
            var updateEntity = await obj.InsertEntity<BaseCosmosEntity>(new MockBaseCosmosEntity()
            {
                Id = "mockId",
                Name = "mockName"
            });
            var removeEntity = await obj.RemoveEntity<BaseCosmosEntity>(It.IsAny<string>());

            Assert.Equal("BaseEntity", insertEnity.Type);
            Assert.Equal("BaseEntity", updateEntity.Type);
            Assert.Equal("BaseEntity", removeEntity.Type);

        }

        public class MockBaseCosmosEntity : BaseCosmosEntity
        {
            public new string? Id { get; set; }
            public string? Name { get; set; }

        }
    }
}
