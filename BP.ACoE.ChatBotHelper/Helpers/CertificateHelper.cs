using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Settings;
using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Helpers
{
    public static class CertificateHelper
    {
        private static readonly object LockCertAccess = new();

        public static bool ValidateCertificate(X509Certificate2? cert)
        {
            return cert is not null && cert.NotAfter >= DateTime.Now && cert.GetRSAPrivateKey() is not null && cert.GetRSAPublicKey() is not null;
        }

        /// <summary>
        /// Downloads the and save new PFX.
        /// </summary>
        /// <param name="salesForceSettings">The sales force settings.</param>
        /// <param name="azureKeyVaultSettings">The azure key vault settings.</param>
        /// <returns></returns>
        public static async Task<X509Certificate2?> DownloadAndSaveNewPfx(SalesForceCertificateSettings salesForceSettings, AzureKeyVaultSettings azureKeyVaultSettings)
        {
            salesForceSettings.Validate();
            azureKeyVaultSettings.Validate();

            var token = await GenerateAzureAccessToken(azureKeyVaultSettings.AzureAuthUrl!, azureKeyVaultSettings.AppClientId!, azureKeyVaultSettings.AppClientSecret!, azureKeyVaultSettings.AzureKeyVaultScope!);
            var certificate = await DownloadPfxCertificate(salesForceSettings.SalesForceCertPath!, salesForceSettings.SalesForceCertName!, salesForceSettings.SalesForceCertPassword!, accessToken: token, azureKeyVaultSettings.KeyVaultCertUri!, azureKeyVaultSettings.CertVersion!);
            return certificate;
        }

        // Add thread lock logic
        private static void GeneratePfxFile(string path, string certName, string? rsaPublicKey, string? privateKey)
        {
            lock (LockCertAccess)
            {
                try
                {
                    var certPath = Path.Combine(path, certName);
                    if (File.Exists(certPath))
                        File.Delete(certPath);

                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    var encodedText = Convert.FromBase64String(privateKey!);

                    //  write the private key to file
                    using (var sourceStream = new FileStream(certPath, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                    {
                        sourceStream.Write(encodedText, 0, encodedText.Length);
                    }
                    var publicPart = Convert.FromBase64String(rsaPublicKey!);

                    using var stream = new FileStream(certPath, FileMode.Append, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: false);
                    stream.Write(publicPart, 0, publicPart.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Download Certificate in PFX Format
        /// </summary>
        /// <param name="path">Directory path of where the certificate will be saved e.g. (c:/certs/)</param>
        /// <param name="certName">Certificate file name with .pfx extension e.g.(SaleForceCert.pfx)</param>
        /// <param name="password"></param>
        /// <param name="accessToken">Azure KeyVault Access Token</param>
        /// <param name="certUrl">Certificate URL in Azure Key Vault</param>
        /// <param name="certVersion">Cert API version query string (required ?api-version=7.0) </param>
        public static async Task<X509Certificate2?> DownloadPfxCertificate(string path, string certName, string password, string? accessToken, string certUrl, string certVersion)
        {

            var certObj = await GetCertificateCerPart($"{certUrl}{certVersion}", accessToken);
            var privateKey = await GetCertificatePrivateKey($"{certObj?.Kid!.ToString().Replace("keys", "secrets")}{certVersion}", accessToken);
            GeneratePfxFile(path, certName, certObj?.Cer, privateKey?.Value);
            var certFullPath = Path.Combine(path, certName);
            var cert = File.Exists(certFullPath) ? new X509Certificate2(await File.ReadAllBytesAsync(certFullPath), password, X509KeyStorageFlags.UserKeySet |
                         X509KeyStorageFlags.PersistKeySet |
                         X509KeyStorageFlags.Exportable) : null;
            return cert;
        }

        private static async Task<CertificateResponse?> GetCertificateCerPart(string certUri,
            string? accessToken)
        {
            return await GetResponse<CertificateResponse>(certUri, accessToken);
        }

        private static async Task<PrivateKeyResponse?> GetCertificatePrivateKey(string keyUri, string? accessToken)
        {
            return await GetResponse<PrivateKeyResponse>(keyUri, accessToken);
        }

        private static async Task<T?> GetResponse<T>(string certUri, string? accessToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var request = new HttpRequestMessage(HttpMethod.Get, certUri);

            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                // do the json parsing
                var accessResp = JsonConvert.DeserializeObject<T>(responseJson);
                return accessResp;
            }
            else
            {
                var opportunityResponseData = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(opportunityResponseData);
            }

        }


        /// <summary>
        /// Generate an access token using Azure AD App client secret and password
        /// </summary>
        /// <param name="authUrl">Auth Url for Azure AD App</param>
        /// <param name="clientId">Client ID (application id of Azure AD App)</param>
        /// <param name="clientSecret">Client Password of Azure AD App</param>
        /// <param name="scope">scope need to for access token e.g. graph, keyvault etc</param>
        /// <returns>A valid token or error message starting with Error:</returns>
        public static async Task<string?> GenerateAzureAccessToken(string authUrl, string clientId, string clientSecret, string scope)
        {
            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, authUrl)
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new("grant_type","client_credentials"),
                    new("client_id",clientId),
                    new("client_secret",clientSecret),
                    new("scope",scope),
                })
            };

            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var accessResp = JsonConvert.DeserializeObject<AccessTokenResponse>(responseJson);
                var accessToken = accessResp?.AccessToken;
                return accessToken;
            }
            else
            {
                var opportunityResponseData = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(opportunityResponseData);
            }
        }
    }
}
