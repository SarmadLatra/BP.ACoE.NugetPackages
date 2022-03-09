using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Settings;
using Moq;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.HelpersUnitTests
{
    public class ObjectToDictionaryExtensionUnitTest
    {
        [Fact]
        public async void TestSuccessSalesForceAuthService()
        {
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => CertificateHelper.DownloadPfxCertificate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            _ = await Assert.ThrowsAsync<HttpRequestException>(() =>
            CertificateHelper.DownloadAndSaveNewPfx(new SalesForceCertificateSettings()
            {
                SalesForceCertName = "mockCertName",
                SalesForceCertPassword = "mocKPassword",
                SalesForceCertPath = "https://mockUrl"
            }, new AzureKeyVaultSettings()
            {
                AppClientId = "mockClientId",
                AppClientSecret = "mockSecret",
                AzureAuthUrl = "https://mockUrl",
                AzureKeyVaultScope = "mockScope",
                KeyVaultCertUri = "mockCertUri"
            }));

        }
    }
}
