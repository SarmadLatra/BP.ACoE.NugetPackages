using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.Services;
using BPMeAUChatBot.API.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using Serilog;
using Xunit;

namespace BPMeAUChatBot.API.Tests.ServicesUnitTest.cs
{
    public class ChatTranscriptServiceUnitTest
    {
        [Fact]
        public async Task TestChatTranscriptService()
        {
            var mockIChatTranscriptStore = new Mock<ITranscriptStore>();
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var configuration = new Mock<IConfiguration>();
            var mockHttpClient = new Mock<HttpClient>();
            var mockTransactionService = new Mock<IChatTransactionService>();
            var mockStorageService = new Mock<IStorageService>();
            var mockEncryptionService = new Mock<IEncryptionService>();
            var mockEmailService = new Mock<IEmailService>();
            var mockChatTransactionService = new Mock<IChatTransactionService>();
            var mockChatTranscriptService = new Mock<IChatTranscriptService>();
            var mockTranscriptStoreService = new Mock<ITranscriptStore>();
            var mockFuncTranscriptStoreService = new Mock<Func<ITranscriptStore>>();
            var mockSettings = new Mock<IOptions<ChatTranscriptSettings>>();

            var azureStorageMock = new Mock<IConfigurationSection>();
            var azureBlobMock = new Mock<IConfigurationSection>();
            mockSettings.Setup(x => x.Value).Returns(new ChatTranscriptSettings()
            {
                ChatbotName = "devraubotsvc",
                SendTranscriptTable = "mock",
                PartitionKey = "mock",
                RewardEmailTemplatePath = "mock",
                StationEmailTemplatePath = "mock",
                GeneralFleetEmailTemplatePath = "mock",
                FleetEmailTemplatePath = "mock",
                CustomerSupportEmailTemplatePath = "mock",
                ChatTranscriptPDFHeaderImagePath = "mock",
                ChatTranscriptPDFFooterImagePath = "mock",
                TimeZone = "AUS Eastern Standard Time",
                EmailFromAddress = "mock",
                TestEnvironment = true,
                SendCCEmail = "true",
                EmailCCAddress = "anant.kulkarni@bp.com",
                ChatBotTranscriptName = "Virtual Assistant",
                EmailTemplatePath = "templates/transcriptEmail.txt",

                FLEET_FORMToEmail = "mock@bp.com",
                REWARD_FORMToEmail = "mock@bp.com",
                PAYMENT_FORMToEmail = "mock@bp.com",
                APP_TECH_FORMToEmail = "mock@bp.com",
                CHANGE_EMAIL_FORMToEmail = "mock@bp.com",
                CLOSE_ACCOUNT_FORMToEmail = "mock@bp.com",
                STATION_ISSUE_FORMToEmail = "mock@bp.com",
                CLICKCOLLECT_RELATEDToEmail = "mock@bp.com",
                ACCOUNT_RELATEDToEmail = "mock@bp.com",
                PAYMENT_RELATEDToEmail = "mock@bp.com",
                REWARDS_RELATEDToEmail = "mock@bp.com",
                STATION_RELATEDToEmail = "mock@bp.com",
                APP_RELATEDToEmail = "mock@bp.com",
                OTHER_QUERYToEmail = "mock@bp.com",
                FLEET_RELATEDToEmail = "mock@bp.com"
            });
            mockIChatTranscriptStore.Setup(x => x.GetTranscriptActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new PagedResult<IActivity>()
            {
                ContinuationToken = null,
            });
            azureStorageMock.Setup(x => x.Value).Returns("mock");
            configuration.Setup(x => x.GetSection("AzureStorageConnectionString")).Returns(azureStorageMock.Object);

            azureBlobMock.Setup(x => x.Value).Returns("mock");
            configuration.Setup(x => x.GetSection("AzureBlobStorageContainer")).Returns(azureBlobMock.Object);

            mockChatTranscriptService.Setup(x => x.GetChatTranscriptFromStore(It.IsAny<string>(), It.IsAny<ITranscriptStore>(), It.IsAny<string>()))
            .ReturnsAsync(new List<Activity>()
            {

            });
            mockChatTransactionService.Setup(x => x.GetTransactionByConversationId(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new Models.ChatbotTransactionEntity()
                {
                    UserId = "dl_0202",
                    Name = "mockName",
                    Email = "mockEmail"
                });
            mockEncryptionService.Setup(x => x.Decrypt(It.IsAny<string>())).Returns("mockDecryptedEmail");

            mockTranscriptStoreService.Setup(x => x.GetTranscriptActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
             .ReturnsAsync(new PagedResult<IActivity>()
             {
                 Items = new IActivity[] { new Activity() { Type = "message", Text = "mockText", Timestamp = new DateTimeOffset(), From = new ChannelAccount() { Name = "mockName" } } },
                 ContinuationToken = null,
             });
            mockEmailService.Setup(x => x.SendHtmlEmailAsync(It.IsAny<Message>()));

            var obj = new ChatTranscriptService(mockSettings.Object, mockLogger, mockHttpClient.Object, mockStorageService.Object,
                                                mockEncryptionService.Object, mockEmailService.Object, mockChatTransactionService.Object, mockTranscriptStoreService.Object);

            var output2 = await obj.GetChatTranscriptFromStore("mockConversationId", mockIChatTranscriptStore.Object, "mockChannel");
            _ = await Assert.ThrowsAsync<QuestPDF.Drawing.Exceptions.DocumentComposeException>(() => obj.SendChatTranscriptAsync(new Models.ChatBotSeibelEntity()
            {
                PartitionKey = "BPmeChatBot",
                RowKey = "9570f80d-dce3-4560-83c0-4081be67f134",
                Timestamp = new DateTimeOffset(),
                ETag = new Azure.ETag() { },
                UserId = "dl_0053M000000tDCXQA2_4d3c978e-b49b-40d2-b01c-3e77b91fc955",
                ConversationId = "3uSkp9dfn6FGREn8kejWAX-p",
                Type = "GENERAL_FORM",
                Comments = "Test - food is gross",
                UserIntent = "support#support",
                UserQuery = "support",
                TxTimestamp = new DateTime(),
                Status = "WEBFORM_SUBMIT",
                EmailStatus = "NEW",
                StationName = "string",
                IssueType = "STATION_RELATED",
                NewEmail = "sarmad.saeed@bp.com",
                OldEmail = "sarmad.saeed@bp.com"
            }));
            _ = await Assert.ThrowsAsync<QuestPDF.Drawing.Exceptions.DocumentComposeException>(() => obj.SendChatTranscriptAsync(new ChatTranscript()
            {
                PartitionKey = "BPmeChatBot",
                RowKey = "9570f80d-dce3-4560-83c0-4081be67f134",
                Timestamp = new DateTimeOffset(),
                ETag = new Azure.ETag() { },
                UserId = "dl_0053M000000tDCXQA2_4d3c978e-b49b-40d2-b01c-3e77b91fc955",
                ConversationId = "3uSkp9dfn6FGREn8kejWAX-p",
                TxTimestamp = "mockTime",
                Status = "WEBFORM_SUBMIT",
            }));
        }
    }
}

