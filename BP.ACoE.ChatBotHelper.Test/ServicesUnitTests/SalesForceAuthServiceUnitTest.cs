using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class SalesForceAuthServiceUnitTest
    {
        private static async Task<X509Certificate2?> CreateCertificateAsync()
        {
            string certificateString =
                @"..\..\..\Certificate\bpmeauchatbot.pfx";

            return File.Exists(certificateString) ? new X509Certificate2(await File.ReadAllBytesAsync(certificateString), "", X509KeyStorageFlags.UserKeySet |
                       X509KeyStorageFlags.PersistKeySet |
                       X509KeyStorageFlags.Exportable) : null;
        }

        [Fact]
        public async void TestSuccessSalesForceAuthService()
        {
            var mockConfiguration = new Mock<IConfiguration>();
            var mockCertificateHelper = new Mock<ISalesForceCertificateHelper>();
            var mockCertificateSetting = new Mock<IOptions<SalesForceCertificateSettings>>();
            var mockKeyVaultSetting = new Mock<IOptions<AzureKeyVaultSettings>>();
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockHttpClient = new Mock<HttpClient>();
            var mockAppInsight = new Mock<IAppInsightsService>();
            var iss = new Mock<IConfigurationSection>();
            var aud = new Mock<IConfigurationSection>();
            var sub = new Mock<IConfigurationSection>();
            var expiry = new Mock<IConfigurationSection>();
            var sfUrl = new Mock<IConfigurationSection>();

            var mockMemoryCache = Create.MockedMemoryCache();

            mockMemoryCache.Set("SFAuthCache", It.IsAny<SalesForceAuthResponse>(), new DateTimeOffset());

            iss.Setup(x => x.Value).Returns("fakeiss");
            aud.Setup(x => x.Value).Returns("fake.salesforce.com");
            sub.Setup(x => x.Value).Returns("fake.customers");
            expiry.Setup(x => x.Value).Returns("24");
            sfUrl.Setup(x => x.Value).Returns("http://fakeurl");

            mockConfiguration.Setup(x => x.GetSection("SalesForceIss")).Returns(iss.Object);
            mockConfiguration.Setup(x => x.GetSection("SalesForceAud")).Returns(aud.Object);
            mockConfiguration.Setup(x => x.GetSection("SalesForceSub")).Returns(sub.Object);
            mockConfiguration.Setup(x => x.GetSection("SalesForceTokenExpiry")).Returns(expiry.Object);
            mockConfiguration.Setup(x => x.GetSection("SalesForceAuthUrl")).Returns(sfUrl.Object);

            mockKeyVaultSetting.Setup(x => x.Value).Returns(new AzureKeyVaultSettings()
            {
                AppClientId = "mockClientId",
                AppClientSecret = "mockSecret",
                AzureAuthUrl = "https://mockUrl",
                AzureKeyVaultScope = "mockScope",
                KeyVaultCertUri = "mockCertUri"
            });

            mockCertificateSetting.Setup(x => x.Value).Returns(new SalesForceCertificateSettings()
            {
                SalesForceCertName = "mockCertName",
                SalesForceCertPassword = "mockPassword",
                SalesForceCertPath = "https://mockPath"
            });

            var certificate =await CreateCertificateAsync();
            mockCertificateHelper.Setup(x => x.Certificate).Returns(certificate);

            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())

                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new SalesForceAuthResponse()
                       {
                           AccessToken = "mockToken",
                           IdToken = "mockId",
                           TokenType = "mockType",
                           InstanceUrl = "https://mockUrl",
                           Scope = "mockScope"
                       })),
                   })
               .Verifiable();

            var httpClient = new HttpClient(mockHandler.Object);

            var service = new SalesForceAuthService(
                mockConfiguration.Object, httpClient, mockLogger, mockCertificateHelper.Object, mockMemoryCache,
              mockKeyVaultSetting.Object
                , mockCertificateSetting.Object, mockAppInsight.Object);

            var result = await service.DoSalesForceAuth();

            Assert.Equal("mockToken", result?.AccessToken);
        }
    }
}
