using System.Net.Http.Headers;
using System.Text;
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
        private const string SfApiVersionKey = "X-LIVEAGENT-API-VERSION";
        private const string SfApiAffinityKey = "X-LIVEAGENT-AFFINITY";
        private const string SfApiSessionKey = "X-LIVEAGENT-SESSION-KEY";
        private const string SfApiSequenceKey = "X-LIVEAGENT-SEQUENCE";
        private const string ClassName = "SalesForceService--";
        private readonly IAppInsightsService _appInsightsCustomEventService;

        public SalesForceLiveAgentService(HttpClient httpClient, ILogger logger, IOptions<SalesForceLiveAgentSettings> liveAgentOptions, IAppInsightsService appInsightsCustomEventService)
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

        public virtual async Task<string> EndLiveAgentChatSession(ChatEndModel model)
        {
            const string methodName = "EndLiveAgentChatSession--";

            _logger.Information($"{ClassName}{methodName} end chat session is called");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _salesForceLiveAgentSettings.ChatEndUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    reason = model.Reason
                }), Encoding.UTF8, "application/json")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, model.AffinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, model.SessionKey);

            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} End Chat API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} End Chat success response received ");
                var responseData = await response.Content.ReadAsStringAsync();

                _logger.Information($"{ClassName}{methodName} End Chat API response serialized into model {responseData}");
                return responseData;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} Sales Force End Chat request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} End Chat request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task<string> SendChatMessageToAgent(ChatMessageModel model)
        {
            const string methodName = "SendChatMessageToAgent--";
            _logger.Information($"{ClassName}{methodName} started");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _salesForceLiveAgentSettings.SendChatMessageUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    text = model.Message
                }), Encoding.UTF8, "application/json")
            };
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, model.AffinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, model.SessionKey);

            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} Send Chat Message API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} success response received ");
                var responseData = await response.Content.ReadAsStringAsync();

                _logger.Information($"{ClassName}{methodName} response serialized into model {responseData}");
                return responseData;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} End Chat request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} End Chat request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task<LiveAgentMessageResponse?> GetCurrentMessagesFromLiveAgent(string sessionId, string affinityToken)
        {
            const string methodName = "GetCurrentMessageFromLiveAgent--";
            _logger.Information($"{ClassName}{methodName} started");

            _client.DefaultRequestHeaders.Clear();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _salesForceLiveAgentSettings.GetChatMessagesUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, affinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, sessionId);


            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} success response received ");
                var responseData = await response.Content.ReadAsStringAsync();
                var parseResponse = JsonConvert.DeserializeObject<LiveAgentMessageResponse>(responseData);
                _logger.Information($"{ClassName}{methodName} response serialized into model {responseData}");
                return parseResponse;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task<string> StartChatWithLiveAgent(StartChatModel model)
        {
            const string methodName = "StartChatWithLiveAgent--";

            _logger.Information($"{ClassName}{methodName} started");

            var content = JsonConvert.SerializeObject(value: new
            {
                organizationId = _salesForceLiveAgentSettings.OrganizationId,
                deploymentId = _salesForceLiveAgentSettings.DeploymentId,
                buttonId = _salesForceLiveAgentSettings.ButtonId,
                sessionId = model.SessionId,
                userAgent = "Lynx/2.8.8",
                language = "en-US",
                screenResolution = "1900x1080",
                visitorName = model.VisitorName,
                prechatDetails = model.PreChatDetails,
                prechatEntities = model.PreChatEntities,
                receiveQueueUpdates = model.ReceiveQueueUpdates,
                isPost = model.IsPost
            });

            _logger.Information($"{ClassName}{methodName} Request payload {content}");


            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _salesForceLiveAgentSettings.StartChatUrl)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, model.AffinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, model.SessionKey);
            _client.DefaultRequestHeaders.Add(SfApiSequenceKey, "1");

            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} success response received ");
                var responseData = await response.Content.ReadAsStringAsync();

                _logger.Information($"{ClassName}{methodName} response serialized into model {responseData}");
                return responseData;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task<LiveAgentSession?> GetLiveAgentSessions()
        {
            const string methodName = "GetLiveAgentSessions--";
            _logger.Information($"{ClassName}{methodName} started");

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _salesForceLiveAgentSettings.LiveAgentSessionUrl);
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, "null");
            _client.DefaultRequestHeaders.Add(SfApiSequenceKey, "1");
            
            var response = await _client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} success response received ");
                var responseData = await response.Content.ReadAsStringAsync();
                var parseResponse = JsonConvert.DeserializeObject<LiveAgentSession>(responseData);
                //Raise event
                var liveAgentData = new
                {
                    parseResponse?.Id,
                    Message = "Live Agent Session Connected"
                };
                await _appInsightsCustomEventService.RaiseGenericCustomEvent($"Live Agent session received {Convert.ToString(liveAgentData.Id)} with message: {liveAgentData.Message}");
                _logger.Information($"{ClassName}{methodName} response serialized into model {responseData}");
                return parseResponse;
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task<LiveAgentResponse?> GetAvailableLiveAgents()
        {
            const string methodName = "GetAvailableLiveAgents--";
            _logger.Information($"{ClassName}{methodName} started");


            _client.DefaultRequestHeaders.Clear();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, _salesForceLiveAgentSettings.AvailableLiveAgentsUrl);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, "");


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
                await _appInsightsCustomEventService.RaiseGenericCustomEvent($"{ClassName}{methodName} failed with {responseData}");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task SendTypingEventToAgent(string sessionKey, string affinityToken)
        {
            const string methodName = "SendTypingEventToAgent--";

            _logger.Information($"{ClassName}{methodName} end chat session is called");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _salesForceLiveAgentSettings.ChatEndUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                }), Encoding.UTF8, "application/json")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, affinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, sessionKey);

            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} End Chat API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName} End Chat success response received ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} End Chat API response serialized into model {responseData}");
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} Sales Force End Chat request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} End Chat request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }

        public virtual async Task SendNotTypingEventToAgent(string sessionKey, string affinityToken)
        {
            const string methodName = "SendNotTypingEventToAgent--";

            _logger.Information($"{ClassName}{methodName} is called");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _salesForceLiveAgentSettings.ChatEndUrl)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                }), Encoding.UTF8, "application/json")
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add(SfApiVersionKey, _salesForceLiveAgentSettings.LiveAgentApiVersion);
            _client.DefaultRequestHeaders.Add(SfApiAffinityKey, affinityToken);
            _client.DefaultRequestHeaders.Add(SfApiSessionKey, sessionKey);

            _logger.Information($"{ClassName}{methodName} Request payload is created");
            var response = await _client.SendAsync(requestMessage);
            _logger.Information($"{ClassName}{methodName} Not Typing API is called");
            if (response.IsSuccessStatusCode)
            {
                _logger.Information($"{ClassName}{methodName}  success response received ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} Not Typing API response serialized into model {responseData}");
            }
            else
            {
                _logger.Information($"{ClassName}{methodName} Sales Force Not Typing request error ");
                var responseData = await response.Content.ReadAsStringAsync();
                _logger.Information($"{ClassName}{methodName} request error {responseData} ");
                throw new HttpRequestException(responseData);
            }
        }
    }
}
