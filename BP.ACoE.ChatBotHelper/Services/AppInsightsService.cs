using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Microsoft.ApplicationInsights;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services;

public class AppInsightsService : IAppInsightsService
{

    private readonly TelemetryClient _telemetry;
    private readonly ILogger _logger;
    private const string ClassName = "AppInsightsService--";


    public AppInsightsService(TelemetryClient telemetry, ILogger logger)
    {
        _telemetry = telemetry;
        _logger = logger.ForContext<AppInsightsService>();
    }

    public virtual Task CustomTypedEventAsync(dynamic customEvent, string? eventName = "Generic Event")
    {
        const string methodName = "ConversationEventAsync--";
        try
        {
            _telemetry.TrackEvent(eventName, ObjectToDictionaryExtension.ToDictionary<string>(customEvent));
        }
        catch (Exception e)
        {
            _logger.Error(e, $"{ClassName} {methodName} failed with error");
        }
        finally
        {
            _telemetry.Flush();
        }

        return Task.CompletedTask;
    }

    public virtual async Task RaiseSaleForceEvent(AppInsightCustomEventTypes eventType, string conversationId, string message)
    {
        var customEvent = new
        {
            conversationId,
            message
        };
        await CustomTypedEventAsync(customEvent, Enum.GetName(typeof(AppInsightCustomEventTypes), eventType));
    }

    public virtual async Task RaiseGetTranscriptsEvent(AppInsightCustomEventTypes eventType, string salesForceCaseNumber)
    {
        var customEvent = new
        {
            salesForceCaseNumber
        };
        await CustomTypedEventAsync(customEvent, Enum.GetName(typeof(AppInsightCustomEventTypes), eventType));
    }

    public virtual async Task RaiseGenericCustomEvent(string message, string conversationId = "", string userId = "")
    {
        dynamic genericEventDetails = new { Message = message };

        if (!string.IsNullOrWhiteSpace(conversationId))
        {

            genericEventDetails = new { Message = message, Conversationid = conversationId };
        }
        if (!string.IsNullOrWhiteSpace(userId))
        {
            genericEventDetails.Add("userId", userId);
        }

        await CustomTypedEventAsync(genericEventDetails, Enum.GetName(typeof(AppInsightCustomEventTypes), AppInsightCustomEventTypes.GenericEvent));
    }
}