using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class AzureAuthService : IAzureAuthService
    {
        private readonly HttpClient _client;
        private readonly ILogger _log;

        private const string ClassName = "AzureAuthService ---";
        private readonly IAppInsightsService _applicationInsightService;

        public AzureAuthService(HttpClient client, ILogger logger, IAppInsightsService applicationInsightService)
        {
            _client = client;
            _log = logger.ForContext<AzureAuthService>();
            _applicationInsightService = applicationInsightService;
        }

        public virtual async Task<AccessTokenResponse?> GetAzureAuthToken(AzureAuthSettings azureAuthSettings)
        {
            const string methodName = "GetAzureAuthToken---";

            _log.Information($"{ClassName}{methodName} Started azure auth token process");
            await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Azure Auth Token process started");

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", azureAuthSettings.GrantType!),
                new KeyValuePair<string, string>("client_id", azureAuthSettings.ClientId!),
                new KeyValuePair<string, string>("client_secret", azureAuthSettings.ClientSecret!),
                azureAuthSettings.IsScope
                    ? new KeyValuePair<string, string>("scope", azureAuthSettings.Scope!)
                    : new KeyValuePair<string, string>("resource", azureAuthSettings.Resource!)
            };

            var postData = new HttpRequestMessage(HttpMethod.Post, azureAuthSettings.AuthAuthUrl)
            {
                Content = new FormUrlEncodedContent(requestData)
            };

            var response = _client.SendAsync(postData).Result;

            _log.Information($"{ClassName}{methodName} request sent");
            await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Azure Auth Token request sent");

            if (response.IsSuccessStatusCode)
            {
                _log.Information($"{ClassName}{methodName} success response received");
                var responseData = await response.Content.ReadAsStringAsync();
                var responseToken = JsonConvert.DeserializeObject<AccessTokenResponse>(responseData);
                _log.Information($"{ClassName}{methodName} Azure auth Access token is generated {responseToken?.AccessToken}");
                await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Azure Auth Token generated");
                _log.Information($"{ClassName}{methodName} response serialized into model ");
                return responseToken;
            }
            else
            {
                _log.Information($"{ClassName}{methodName} request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _log.Information($"{ClassName}{methodName} request error {responseData} ");
                await _applicationInsightService.RaiseGenericCustomEvent(ClassName, methodName, $"Azure Auth Token Request error. Error Message:{responseData}");
                throw new HttpRequestException(responseData);
            }
        }
    }
}
