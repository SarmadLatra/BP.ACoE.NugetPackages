using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class SalesForceLiveAgentService : ISalesForceLiveAgentService
    {

        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly SalesForceLiveAgentSettings _salesForceLiveAgentSettings;
        private const string SF_API_VERSION_KEY = "X-LIVEAGENT-API-VERSION";
        private const string SF_API_AFFINITY_KEY = "X-LIVEAGENT-AFFINITY";
        private const string SF_API_SESSION_KEY = "X-LIVEAGENT-SESSION-KEY";
        private const string SF_API_SEQUENCE_KEY = "X-LIVEAGENT-SEQUENCE";
        private const string ClassName = "SalesForceService--";
        private readonly IAppInsightsCustomEventService _appInsightsCustomEventService;

        public SalesForceLiveAgentService(HttpClient httpClient, ILogger logger, IOptions<SalesForceLiveAgentSettings> liveAgentOptions, IAppInsightsCustomEventService appInsightsCustomEventService)
        {
            _client = httpClient;
            _appInsightsCustomEventService = appInsightsCustomEventService;
            _logger = logger.ForContext<SalesForceLiveAgentService>();
            _salesForceLiveAgentSettings = liveAgentOptions.Value;
            if (_salesForceLiveAgentSettings.BuildAvailableLiveAgentUrl)
            {
                _salesForceLiveAgentSettings.AvailableLiveAgentsUrl = string.Format(_salesForceLiveAgentSettings.AvailableLiveAgentsUrl!, _salesForceLiveAgentSettings.BuildAvailableLiveAgentUrl, _salesForceLiveAgentSettings.DeploymentId, _salesForceLiveAgentSettings.OrganizationId);
            }
        }

        public Task<string> EndLiveAgentChatSession(ChatEndModel model)
        {
            throw new NotImplementedException();
        }

        public Task<string> SendChatMessageToAgent(ChatMessageModel model)
        {
            throw new NotImplementedException();
        }

        public Task<LiveAgentMessageResponse> GetCurrentMessagesFromLiveAgent(string sessionId, string affinityToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> StartChatWithLiveAgent(StartChatModel model)
        {
            throw new NotImplementedException();
        }

        public Task<LiveAgentSession> GetLiveAgentSessions()
        {
            throw new NotImplementedException();
        }

        public async Task<LiveAgentResponse?> GetAvailableLiveAgents()
        {
            const string methodName = "GetAvailableLiveAgents--";
            _logger.Information($"{ClassName}{methodName} started");


            _client.DefaultRequestHeaders.Clear();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _salesForceLiveAgentSettings.AvailableLiveAgentsUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SF_API_VERSION_KEY, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SF_API_AFFINITY_KEY, "");


            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} success response received ");
                var responseData = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonConvert.DeserializeObject<LiveAgentResponse>(responseData);
                _logger.Information($"{ClassName}{methodName} response serialized into model {responseData}");
                return parsedResponse;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} request error {responseData} ");
                await _appInsightsCustomEventService.RaiseGenericCustomEvent(className: ClassName, methodName, $"failed with {responseData}");
                throw new HttpRequestException(responseData);
            }
        }

        public Task SendTypingEventToAgent(string sessionKey, string affinityToken)
        {
            throw new NotImplementedException();
        }

        public Task SendNotTypingEventToAgent(string sessionKey, string affinityToken)
        {
            throw new NotImplementedException();
        }
    }
}
