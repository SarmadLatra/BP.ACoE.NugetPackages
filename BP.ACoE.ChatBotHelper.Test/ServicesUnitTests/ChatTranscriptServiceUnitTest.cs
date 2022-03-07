using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.Services;
using BPMeAUChatBot.API.Services.Interfaces;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
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

            var azureStorageMock = new Mock<IConfigurationSection>();
            var azureBlobMock = new Mock<IConfigurationSection>();
            var ChatTranscriptPDFHeaderImagePathMock = new Mock<IConfigurationSection>();
            var ChatTranscriptPDFFooterImagePathMock = new Mock<IConfigurationSection>();
            var TimeZoneMock = new Mock<IConfigurationSection>();

            var RewardEmailTemplatePathMock = new Mock<IConfigurationSection>();
            var StationEmailTemplatePathMock = new Mock<IConfigurationSection>();
            var GeneralFleetEmailTemplatePathMock = new Mock<IConfigurationSection>();
            var FleetEmailTemplatePathMock = new Mock<IConfigurationSection>();
            var CustomerSupportEmailTemplatePathMock = new Mock<IConfigurationSection>();
            var EmailTemplatePathMock = new Mock<IConfigurationSection>();

            var ChatbotNameMock = new Mock<IConfigurationSection>();
            var EmailFromAddressMock = new Mock<IConfigurationSection>();
            var TestEnvironmentMock = new Mock<IConfigurationSection>();
            var SendCCEmailMock = new Mock<IConfigurationSection>();
            var SendEmailCCMock = new Mock<IConfigurationSection>();

            mockIChatTranscriptStore.Setup(x => x.GetTranscriptActivitiesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTimeOffset>()))
            .ReturnsAsync(new PagedResult<IActivity>()
            {
                ContinuationToken = null,
            });

            EmailTemplatePathMock.Setup(x => x.Value).Returns("templates/transcriptEmail.txt");
            configuration.Setup(x => x.GetSection("EmailTemplatePath")).Returns(EmailTemplatePathMock.Object);

            ChatbotNameMock.Setup(x => x.Value).Returns("mockChatBotName");
            configuration.Setup(x => x.GetSection("ChatbotName")).Returns(ChatbotNameMock.Object);

            EmailFromAddressMock.Setup(x => x.Value).Returns("mockEmailId");
            configuration.Setup(x => x.GetSection("EmailFromAddress")).Returns(EmailFromAddressMock.Object);

            TestEnvironmentMock.Setup(x => x.Value).Returns("false");
            configuration.Setup(x => x.GetSection("TestEnvironment")).Returns(TestEnvironmentMock.Object);

            SendCCEmailMock.Setup(x => x.Value).Returns("true");
            configuration.Setup(x => x.GetSection("SendCCEmail")).Returns(SendCCEmailMock.Object);

            SendEmailCCMock.Setup(x => x.Value).Returns("mockCCEmail");
            configuration.Setup(x => x.GetSection("EmailCCAddress")).Returns(SendEmailCCMock.Object);

            RewardEmailTemplatePathMock.Setup(x => x.Value).Returns("templates/RewardsEmail.txt");
            configuration.Setup(x => x.GetSection("RewardEmailTemplatePath")).Returns(RewardEmailTemplatePathMock.Object);

            StationEmailTemplatePathMock.Setup(x => x.Value).Returns("templates/CarelineEmail.txt");
            configuration.Setup(x => x.GetSection("StationEmailTemplatePath")).Returns(StationEmailTemplatePathMock.Object);

            GeneralFleetEmailTemplatePathMock.Setup(x => x.Value).Returns("templates/GeneralFleet.txt");
            configuration.Setup(x => x.GetSection("GeneralFleetEmailTemplatePath")).Returns(GeneralFleetEmailTemplatePathMock.Object);

            FleetEmailTemplatePathMock.Setup(x => x.Value).Returns("templates/FleetEmail.txt");
            configuration.Setup(x => x.GetSection("FleetEmailTemplatePath")).Returns(FleetEmailTemplatePathMock.Object);

            CustomerSupportEmailTemplatePathMock.Setup(x => x.Value).Returns("templates/FleetEmail.txt");
            configuration.Setup(x => x.GetSection("CustomerSupportEmailTemplatePath")).Returns(CustomerSupportEmailTemplatePathMock.Object);

            TimeZoneMock.Setup(x => x.Value).Returns("AUS Eastern Standard Time");
            configuration.Setup(x => x.GetSection("TimeZone")).Returns(TimeZoneMock.Object);

            ChatTranscriptPDFHeaderImagePathMock.Setup(x => x.Value).Returns("\\templates\\TemplateHeader.png");
            configuration.Setup(x => x.GetSection("ChatTranscriptPDFHeaderImagePath")).Returns(ChatTranscriptPDFHeaderImagePathMock.Object);

            ChatTranscriptPDFFooterImagePathMock.Setup(x => x.Value).Returns("\\templates\\TemplateFooter.png");
            configuration.Setup(x => x.GetSection("ChatTranscriptPDFFooterImagePath")).Returns(ChatTranscriptPDFFooterImagePathMock.Object);

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

            var obj = new ChatTranscriptService(configuration.Object, mockLogger, mockHttpClient.Object, mockStorageService.Object,
                                                mockEncryptionService.Object, mockEmailService.Object, mockChatTransactionService.Object, mockTranscriptStoreService.Object);

            var output1 = obj.GetBlobsTranscriptStore();
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

