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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class SalesForceLiveAgentServiceUnitTest
    {
        [Fact]
        public async void TestSalesForceService()
        {
            var service = SetParam(new object());

            var result1 = await service.EndLiveAgentChatSession(new Models.ChatEndModel()
            {
                AffinityToken = "mockToken",
                Reason = "mockReason",
                SessionKey = "mockKey"
            });

            var result2 = await service.SendChatMessageToAgent(new Models.ChatMessageModel()
            {
                AffinityToken = "mockToken",
                SessionKey = "mockKey"
            });

            var result3 = await service.StartChatWithLiveAgent(new Models.StartChatModel()
            {
                SessionId = "mockSession",
                VisitorName = "mockVisitor",
                ReceiveQueueUpdates = true
            });

            await service.SendTypingEventToAgent("session", "affinity");

            await service.SendNotTypingEventToAgent("session", "affinity");

            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotNull(result3);
        }

        [Fact]
        public async Task TestCurrentMessagesFromLiveAgentAsync()
        {
            var liveAgentMessageResponse = new LiveAgentMessageResponse
            {
                Messages = new MessageElement[]
            {
                new MessageElement() { Message=new MessageMessage(){ AgentId="mockid1" },Type="mockType" },
                new MessageElement() { Message=new MessageMessage(){ AgentId="mockid2" },Type="mockType" }
            },
                Offset = 0,
                Sequence = 0
            };
            var service = SetParam(liveAgentMessageResponse);

            var result = await service.GetCurrentMessagesFromLiveAgent("session", "affinity");

            Assert.Equal("mockid1", result?.Messages?.FirstOrDefault()?.Message?.AgentId);
        }

        [Fact]
        public async Task TestGetLiveAgentSessions()
        {
            var liveAgentSession = new LiveAgentSession
            {
                AffinityToken = "mockToken",
                ClientPollTimeout = 0,
                Id = new Guid(),
                Key = "mockKey"
            };

            var service = SetParam(liveAgentSession);

            var result = await service.GetLiveAgentSessions();

            Assert.Equal("mockToken", result?.AffinityToken);
        }

        [Fact]
        public async Task TestGetAvailableLiveAgents()
        {
            var liveAgentResponse = new LiveAgentResponse
            {
                Messages = new MessageElement[]
           {
                new MessageElement() { Message=new MessageMessage(){ AgentId="mockid1" },Type="mockType" },
                new MessageElement() { Message=new MessageMessage(){ AgentId="mockid2" },Type="mockType" }
           }
            };

            var service = SetParam(liveAgentResponse);

            var result = await service.GetAvailableLiveAgents();

            Assert.Equal("mockid1", result?.Messages?.FirstOrDefault()?.Message?.AgentId);
        }


        public static SalesForceLiveAgentService SetParam(object type)
        {
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var appInsights = new Mock<IAppInsightsService>();
            var mockSettings = new Mock<IOptions<SalesForceLiveAgentSettings>>();
            mockSettings.Setup(x => x.Value).Returns(new SalesForceLiveAgentSettings()
            {
                AvailableLiveAgentsUrl = "https://mockUrls",
                BuildAvailableLiveAgentUrl = true,
                ButtonId = "mockButtonId",
                ChatEndUrl = "https://mockUrls",
                DeploymentId = "mockDeploymentId",
                GetChatMessagesUrl = "https://mockUrls",
                LiveAgentApiVersion = "mockVersion",
                LiveAgentSessionUrl = "https://mockUrls",
                OrganizationId = "mockOrgId",
                SendChatMessageUrl = "https://mockUrls",
                StartChatUrl = "https://mockUrls"
            });

            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            string typeName = type.GetType().Name;

            switch (typeName)
            {
                case "LiveAgentResponse":

                    mockHandler.Protected()
                      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())
                  .ReturnsAsync(new HttpResponseMessage()
                  {
                      StatusCode = HttpStatusCode.OK,
                      Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(type)),
                  })
              .Verifiable();
                    break;

                case "LiveAgentSession":

                    mockHandler.Protected()
                      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())

                  .ReturnsAsync(new HttpResponseMessage()
                  {
                      StatusCode = HttpStatusCode.OK,
                      Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(type)),
                  })
              .Verifiable();
                    break;

                case "LiveAgentMessageResponse":

                    mockHandler.Protected()
                      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                   ItExpr.IsAny<CancellationToken>())

                  .ReturnsAsync(new HttpResponseMessage()
                  {
                      StatusCode = HttpStatusCode.OK,
                      Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(type)),
                  })
              .Verifiable();
                    break;

                default:

                    mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{}"),
                    })
          .Verifiable();
                    break;
            }
            var httpClient = new HttpClient(mockHandler.Object);

            return new SalesForceLiveAgentService(httpClient, mockLogger, mockSettings.Object, appInsights.Object);
        }
    }

}