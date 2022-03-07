using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Helpers;
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
