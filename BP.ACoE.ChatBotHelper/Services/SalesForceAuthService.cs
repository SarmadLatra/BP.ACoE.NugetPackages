using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class SalesForceAuthService : ISalesForceAuthService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private const string ClassName = "SalesForceService--";
        private readonly ISalesForceCertificateHelper _salesForceCertificateHelper;
        private readonly IMemoryCache _cache;
        private const string SfAuthCacheName = "SFAuthCache";
        private readonly IAppInsightsCustomEventService _applicationInsightService;
        private readonly AzureKeyVaultSettings? _azureKeyVaultSettings;
        private readonly SalesForceCertificateSettings? _salesForceCertificateSettings;


        public SalesForceAuthService(IConfiguration configuration, HttpClient client, ILogger logger,
            ISalesForceCertificateHelper salesForceCertificateHelper, IMemoryCache cache,
            IOptions<AzureKeyVaultSettings> azureKeyVaultOptions,
            IOptions<SalesForceCertificateSettings> salesForceCertOptions,
            IAppInsightsCustomEventService applicationInsightService)
        {
            _configuration = configuration;
            _client = client;
            _logger = logger.ForContext<SalesForceAuthService>();
            _salesForceCertificateHelper = salesForceCertificateHelper;
            _applicationInsightService = applicationInsightService;
            _cache = cache;
            _azureKeyVaultSettings = azureKeyVaultOptions.Value;
            _salesForceCertificateSettings = salesForceCertOptions.Value;
        }

        public async Task<SalesForceAuthResponse?> GetSalesForceAccessToken()
        {
            const string methodName = "GetSalesForceAccessToken--";

            _logger.Information($"{ClassName}{methodName} Sales Force access token API is called");

            var iss = _configuration.GetValue<string>("SalesForceIss");
            var aud = _configuration.GetValue<string>("SalesForceAud");
            var sub = _configuration.GetValue<string>("SalesForceSub");
            var expiry = _configuration.GetValue<string>("SalesForceTokenExpiry");
            var sfUrl = _configuration.GetValue<string>("SalesForceAuthUrl");

            var jwtPayload = new
            {
                iss,
                aud,
                sub,
                exp = DateTimeOffset.UtcNow.AddHours(int.Parse(expiry)).ToUnixTimeSeconds()
            };

            var jwt = await GenerateJwtWithCertificateFile(jwtPayload);
            _logger.Information($"{ClassName}{methodName} JWT generated");
            // make a call to SalesForce Login API to get Access Token Response

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{sfUrl}")
            {
                Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string?>>()
              {
                new KeyValuePair<string, string?>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new KeyValuePair<string, string?>("assertion", jwt),
              })
            };
            _logger.Information($"{ClassName}{methodName} Auth request content constructed");
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger.Information("Sales Force Request payload is created");

            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} Auth API is called");
            await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Auth API is called");

            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} Auth API success response received ");
                var responseData = await response.Content.ReadAsStringAsync();

                _logger.Information($"{ClassName}{methodName}  Auth API response serialized into model {responseData}");
                var responseToken = JsonConvert.DeserializeObject<SalesForceAuthResponse?>(responseData);
                return responseToken;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} Auth API request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} Auth API request error {responseData} ");
                await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Auth API request Error :{responseData}");
                throw new HttpRequestException(responseData);
            }
        }


        #region Private Methods

        /// <summary>
        /// Does the sales force authentication.
        /// </summary>
        public async Task<SalesForceAuthResponse?> DoSalesForceAuth()
        {
            const string methodName = "DoSalesForceAuth---";
            var salesForceTokenResponse = _cache.Get<SalesForceAuthResponse?>(SfAuthCacheName);
            if (salesForceTokenResponse != null) return salesForceTokenResponse;
            _logger.Information($"{ClassName}{methodName} calls salesForce access token generation process");
            salesForceTokenResponse = await GetSalesForceAccessToken();
            _cache.Set(SfAuthCacheName, salesForceTokenResponse, TimeSpan.FromMinutes(15));
            _logger.Information($"{ClassName}{methodName} process completed successfully");
            return salesForceTokenResponse;
        }

        /// <summary>
        /// Generates the JWT with certificate file.
        /// </summary>
        /// <returns></returns>
        public async Task<string?> GenerateJwtWithCertificateFile(object jwtPayload)
        {
            const string methodName = "GenerateJwtWithCertificateFile--";
            _logger.Information($"{ClassName}{methodName} started generating jwt with certificate file");
            await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Generating jwt with certificate file.");

            try
            {
                if (!CertificateHelper.ValidateCertificate(_salesForceCertificateHelper.Certificate))
                {
                    _salesForceCertificateHelper.Certificate = await CertificateHelper.DownloadAndSaveNewPfx(_salesForceCertificateSettings!, _azureKeyVaultSettings!);
                }

                var algorithm = new RS256Algorithm(_salesForceCertificateHelper.Certificate!.GetRSAPublicKey(), _salesForceCertificateHelper.Certificate!.GetRSAPrivateKey());
                var serializer = new JsonNetSerializer();
                var urlEncoder = new JwtBase64UrlEncoder();
                var encoder = new JwtEncoder(algorithm, serializer, urlEncoder);
                var encryptedToken = encoder.Encode(jwtPayload, Array.Empty<byte>());
                _logger.Information($"{ClassName}{methodName} -- {encryptedToken}");

                return encryptedToken;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Error Generating jwt with certificate file.Error Message:{ex.Message}");
                return null;
            }
        }
        #endregion
    }

}
