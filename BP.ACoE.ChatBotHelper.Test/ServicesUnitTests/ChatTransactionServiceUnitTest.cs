using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.Services;
using BPMeAUChatBot.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Serilog;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BPAURewardsChatBot.API.Tests.ServicesUnitTest.cs
{
    public class ChatTransactionServiceUnitTest
    {
        [Fact]
        public async void Test_Success()
        {
            var configuration = new Mock<IConfiguration>();
            var mockChatTxTable = new Mock<IConfigurationSection>();
            var mockPartitionKey = new Mock<IConfigurationSection>();
            var mockSettings = new Mock<IOptions<ChatTransactionSettings>>();
            mockSettings.Setup(x => x.Value).Returns(new ChatTransactionSettings()
            {
                ChatTxTable = "mockTable",
                PartitionKey = "mockKey"
            });
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockStorageService = new Mock<IStorageService>();

            var entity1 =
              new List<ChatbotTransactionEntity>()
              { new ChatbotTransactionEntity(){
                  ChatStarted = true,
                  ConversationId = "fakeconversationid",
                  BotTransactionCount = 1,
                  RowKey = "fake"
              }, new ChatbotTransactionEntity(){
                  ChatStarted = true,
                  ConversationId = "fakeconversationid",
                  BotTransactionCount = 1,
                  RowKey = "fake"
              } };

            _ = mockStorageService.Setup(x => x.GetEntitiesByQuery<ChatbotTransactionEntity>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(entity1);

            mockChatTxTable.Setup(x => x.Value).Returns("BPmeAUChatBotTxs");
            mockPartitionKey.Setup(x => x.Value).Returns("BPMeAUChatBot");

            configuration.Setup(x => x.GetSection("ChatTxTable")).Returns(mockChatTxTable.Object);
            configuration.Setup(x => x.GetSection("PartitionKey")).Returns(mockPartitionKey.Object);

            var chatTransactionService = new ChatTransactionService(mockSettings.Object, mockLogger, mockStorageService.Object);

            var result = await chatTransactionService.GetTransactionByConversationId("fakeConversationId");

            Assert.Equal("fakeconversationid", result.ConversationId);
        }
    }
}
