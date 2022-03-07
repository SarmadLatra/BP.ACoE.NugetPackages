using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Services;
using BPMeAUChatBot.API.Services.Interfaces;
using BPMeAUChatBot.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using Serilog;
using System;
using System.Threading.Tasks;
using Xunit;

namespace BPAURewardsChatBot.API.Tests.ServicesUnitTest.cs
{
    public class EmailServiceUnitTest
    {
        [Fact]
        public void Test_sendEmail_async_issuccess()
        {
            var configurations = new Mock<IConfiguration>();
            var logger = new LoggerConfiguration().CreateLogger();
            var graphApi = new Mock<IOptions<GraphApiAuthSettings>>();
            var authService = new Mock<IAzureAuthService>();

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            _ = authService.Setup(x => x.GetAzureAuthToken(new AzureAuthSettings()
            {
                ClientId = "mockClientId",
                ClientSecret = "mockClientSecret",
                AuthAuthUrl = "https://url",
                GrantType = "mockType",
                IsScope = true,
                Resource = "mockResource",
                Scope = "mockScope"
            })).Returns(Task.FromResult(new AccessTokenResponse()
            {
                AccessToken = "fakeToken"
            }));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

            var EmailFromAddressMock = new Mock<IConfigurationSection>();
            EmailFromAddressMock.Setup(x => x.Value).Returns("mockemail@gmail.com");
            configurations.Setup(x => x.GetSection("EmailFromAddress")).Returns(EmailFromAddressMock.Object);

            _ = graphApi.Setup(x => x.Value).Returns(new GraphApiAuthSettings()
            {
                ApiUrl = "mockUrl",
                ClientId = "mockClientId",
                ClientSecret = "mockSecret",
                Instance = "mockInstance",
                Tenant = "mockTenant"
            });
            var obj = new EmailService(logger, graphApi.Object, authService.Object);

            Assert.ThrowsAsync<ServiceException>(async () => await obj.SendHtmlEmailAsync(new Microsoft.Graph.Message()));
        }

    }
}
