using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IAppInsightsService
    {
        Task CustomTypedEventAsync(dynamic customEvent, string? eventName = "Generic Event");
        Task RaiseSaleForceEvent(AppInsightCustomEventTypes eventType, string conversationId, string message);
        Task RaiseGetTranscriptsEvent(AppInsightCustomEventTypes eventType, string salesForceCaseNumber);
        Task RaiseGenericCustomEvent(string message, string conversationId = "", string userId = "");
    }
}
