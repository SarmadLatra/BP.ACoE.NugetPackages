using System.Net;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class EncryptionServiceUnitTest
    {
        [Fact]
        public void Test_Encrypt_IsSucess()
        {
            //Arrange
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var configuration = new Mock<IConfiguration>();
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var storageService = new Mock<IStorageService>();
            var mockSettings = new Mock<IOptions<EncryptionSettings>>();

            mockSettings.Setup(x => x.Value).Returns(new EncryptionSettings()
            {
                 EncryptionKey="mockKey"
            });
            mockHandler.Protected()
                       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())

                   .ReturnsAsync(new HttpResponseMessage()
                   {
                       StatusCode = HttpStatusCode.OK,
                       Content = new StringContent("{\r\n\"token\": \"fakeToken\",\r\n  \"conversationId\": \"fakeConversationId\",\r\n    \"expires_in\": 3600\r\n}"),
                   })
               .Verifiable();

            //Act
            var service = new EncryptionService(mockSettings.Object, mockLogger);
            var encryptEmail = service.Encrypt("SampleEmailIdString");
            var decryptEmail = service.Decrypt(encryptEmail);

            //Assert
            Assert.NotNull(encryptEmail);
            Assert.NotNull(decryptEmail);
        }

        [Fact]
        public void TestEncription()
        {
            var configuration = new Mock<IConfiguration>();
            var mockLogger = new LoggerConfiguration().CreateLogger();
            var mockSettings = new Mock<IOptions<EncryptionSettings>>();

            mockSettings.Setup(x => x.Value).Returns(new EncryptionSettings()
            {
                EncryptionKey = "mockKey"
            });
            var mockConfiguration = new Mock<IConfigurationSection>();
            mockConfiguration.Setup(x => x.Value).Returns("MbQeThVmYq3t6w9z$C&F)J@NcRfUjXnZ");
            configuration.Setup(x => x.GetSection("EncryptionKey")).Returns(mockConfiguration.Object);
            var encryptionService = new EncryptionService(mockSettings.Object, mockLogger);

            var original = "mudasar.rauf1@bp.com";
            var encrypted = encryptionService.Encrypt(original);

            Assert.NotNull(encrypted);

            var decrypted = encryptionService.Decrypt(encrypted);
            Assert.NotNull(decrypted);
            Assert.Equal(decrypted, original);
        }
    }
}
