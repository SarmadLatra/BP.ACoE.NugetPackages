using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IAppInsightsCustomEventService
    {
        Task RaiseSaleForceEvent(AppInsightCustomEventTypes eventType, string conversationId, string message);
        Task RaiseGetTranscriptsEvent(AppInsightCustomEventTypes eventType, string salesForceCaseNumber);
        Task RaiseLiveAgentSession(AppInsightCustomEventTypes eventType, string id, string message);
        Task RaiseGenericCustomEvent(string className, string methodName, string message, string conversationId = "", string userId = "");
    }
}
