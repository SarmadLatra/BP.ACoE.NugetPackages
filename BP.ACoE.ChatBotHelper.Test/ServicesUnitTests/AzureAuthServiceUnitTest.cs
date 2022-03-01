using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace BPAURewardsChatBot.API.Tests.ServicesUnitTest.cs
{
    public class AzureAuthServiceUnitTest
    {
        [Fact]
        public async void TestSuccessAzureAuthService()
        {
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockHttpClient = new Mock<HttpClient>();
            var mockAppInsight = new Mock<IAppInsightsService>();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())

                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent("{\r\n\"token\": \"fakeToken\",\r\n  \"conversationId\": \"fakeConversationId\",\r\n    \"expires_in\": 3600\r\n}"),
                   })
               .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var service = new AzureAuthService(httpClient, mockLogger, mockAppInsight.Object);

            var result1 = await service.GetAzureAuthToken(new BP.ACoE.ChatBotHelper.Settings.AzureAuthSettings()
            {
                ClientId = "mockClientId",
                GrantType = "mockGrantType",
                ClientSecret = "mockClientSecret",
                IsScope = true,
                Scope = "mockScope",
                AuthAuthUrl = "https://mockUrl",
                Resource = "mockResource"
            });

            Assert.Equal(3600, result1?.ExpiresIn);
        }

        [Fact]
        public async void TestFailureAzureAuthService()
        {
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockHttpClient = new Mock<HttpClient>();
            var mockAppInsight = new Mock<IAppInsightsService>();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())

                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.BadRequest,
                       Content = new StringContent("{\r\n\"token\": \"fakeToken\",\r\n  \"conversationId\": \"fakeConversationId\",\r\n    \"expires_in\": 3600\r\n}"),
                   })
               .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var service = new AzureAuthService(httpClient, mockLogger, mockAppInsight.Object);

            _ = await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAzureAuthToken(new BP.ACoE.ChatBotHelper.Settings.AzureAuthSettings()
            {
                ClientId = "mockClientId",
                GrantType = "mockGrantType",
                ClientSecret = "mockClientSecret",
                IsScope = true,
                Scope = "mockScope",
                AuthAuthUrl = "https://mockUrl",
                Resource = "mockResource"
            }));
        }
    }
}
