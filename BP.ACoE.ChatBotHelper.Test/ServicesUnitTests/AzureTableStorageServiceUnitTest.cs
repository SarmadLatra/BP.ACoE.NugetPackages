using Azure;
using Azure.Core;
using Azure.Data.Tables;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
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
    public class AzureTableStorageServiceUnitTest
    {
        [Fact]
        public async Task TestAzureTableStorageServiceAsync()
        {
            var obj = SetParam(false);

            var mockType = new { Id = "mockId", Name = "mock" };

            var result1 = await obj.GetStorageTableAsync("mockTable");
            var result2 = obj.GetEntityByRowKey<BaseEntity>("mockTable", "mockRowKey");
            var result3 = await obj.GetEntitiesByQuery<BaseEntity>("mockTable", "mockRowKey");

            await Assert.ThrowsAsync<HttpRequestException>(() => obj.InsertEntity("mockTable", new BaseEntity() { }));
            await Assert.ThrowsAsync<HttpRequestException>(() => obj.UpdateEntity("mockTable", new BaseEntity() { }));
            await Assert.ThrowsAsync<NullReferenceException>(() => obj.RemoveEntity("mockTable", "mockRowKey"));
            await Assert.ThrowsAsync<HttpRequestException>(() => obj.GetEntityByConversationId<BaseEntity>("mockTable", "mockConversation"));

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        [Fact]
        public async Task TestSuccessAzureTableStorageServiceAsync()
        {
            var obj = SetParam(true);

            var result1 =await obj.InsertEntity("mockTable", new BaseEntity()
            {
                DateCreated = new DateTime(),
                ETag = ETag.All,
                PartitionKey = "mockPartitionKey",
                RowKey = "mockRowKey",
                Timestamp = new DateTimeOffset()
            });
            var result2 =await obj.UpdateEntity("mockTable", new BaseEntity()
            {
                DateCreated = new DateTime(),
                ETag = ETag.All,
                PartitionKey = "mockPartitionKey",
                RowKey = "mockRowKey",
                Timestamp = new DateTimeOffset()
            });

            Assert.Equal("mockPartitionKey",result1.PartitionKey);
            Assert.Equal("mockRowKey",result2.RowKey);
        }

        public static AzureTableStorageService SetParam(bool isError)
        {
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockClient = new Mock<TableServiceClient>();
            var mockTableClient = new Mock<TableClient>();
            var mockAzureResponse = new Mock<Response>();
            var mockPageableResponse = new Mock<Pageable<BaseEntity>>();

            _ = isError ? mockAzureResponse.Setup(x => x.IsError).Returns(false) : mockAzureResponse.Setup(x => x.IsError).Returns(true);

            _ = mockTableClient.Setup(x => x.GetEntityAsync<BaseEntity>("mock", "mock", null, default));
            _ = mockTableClient.Setup(x => x.AddEntityAsync(It.IsAny<BaseEntity>(), default)).ReturnsAsync(mockAzureResponse.Object);
            _ = mockTableClient.Setup(x => x.UpdateEntityAsync(It.IsAny<BaseEntity>(), ETag.All, TableUpdateMode.Merge, default)).ReturnsAsync(mockAzureResponse.Object);
            _ = mockTableClient.Setup(x => x.DeleteEntityAsync("mockParitionKey", "mockRow", ETag.All, default)).ReturnsAsync(mockAzureResponse.Object);
            _ = mockTableClient.Setup(x => x.Query<BaseEntity>("filter", 1, null, default)).Returns(mockPageableResponse.Object);
            _ = mockClient.Setup(x => x.GetTableClient(It.IsAny<string>())).Returns(mockTableClient.Object);

            return new AzureTableStorageService(mockClient.Object, mockLogger, "mockPartitionKey");
        }
    }
}
