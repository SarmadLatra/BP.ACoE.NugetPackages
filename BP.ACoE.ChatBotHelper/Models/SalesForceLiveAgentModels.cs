using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class StartChatModel
    {
        [Required]
        public string? SessionId { get; set; }

        [Required]
        public string? SessionKey { get; set; }

        [Required]
        public string? VisitorName { get; set; }

        [JsonProperty("prechatDetails")] public object[] PreChatDetails { get; set; } = new object[] { };
        [JsonProperty("prechatEntities")] public object[] PreChatEntities { get; set; } = new object[] { };
        public bool ReceiveQueueUpdates { get; set; } = true;
        public bool IsPost { get; set; } = true;
        public string? AffinityToken { get; set; }
    }



    public class ChatMessageModel
    {
        [Required]
        public string? SessionKey { get; set; }
        [Required]
        public string? Message { get; set; }

        public string? AffinityToken { get; set; }
    }

    public class ChatEndModel
    {
        [Required]
        public string? SessionKey { get; set; }
        [Required]
        public string? Reason { get; set; }

        public string? AffinityToken { get; set; }
    }

    public class LiveAgentResponse
    {
        [JsonProperty("messages")]
        public MessageElement[]? Messages { get; set; }
    }

    public class LiveAgentMessageResponse
    {
        [JsonProperty("messages")]
        public MessageElement[]? Messages { get; set; }

        [JsonProperty("sequence")]
        public long Sequence { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }
    }

    public class MessageElement
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("message")]
        public MessageMessage? Message { get; set; }
    }

    public class MessageMessage
    {
        [JsonProperty("results")]
        public Result[]? Results { get; set; }

        [JsonProperty("connectionTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public long? ConnectionTimeout { get; set; }

        [JsonProperty("estimatedWaitTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? EstimatedWaitTime { get; set; }

        [JsonProperty("sensitiveDataRules", NullValueHandling = NullValueHandling.Ignore)]
        public object[]? SensitiveDataRules { get; set; }

        [JsonProperty("transcriptSaveEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? TranscriptSaveEnabled { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; set; }

        [JsonProperty("queuePosition", NullValueHandling = NullValueHandling.Ignore)]
        public long? QueuePosition { get; set; }

        [JsonProperty("customDetails", NullValueHandling = NullValueHandling.Ignore)]
        public object[]? CustomDetails { get; set; }

        [JsonProperty("visitorId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? VisitorId { get; set; }

        [JsonProperty("geoLocation", NullValueHandling = NullValueHandling.Ignore)]
        public GeoLocation? GeoLocation { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string? Name { get; set; }

        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore)]
        public string? UserId { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public object[]? Items { get; set; }

        [JsonProperty("sneakPeekEnabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SneakPeekEnabled { get; set; }

        [JsonProperty("chasitorIdleTimeout", NullValueHandling = NullValueHandling.Ignore)]
        public ChasitorIdleTimeout? ChasitorIdleTimeout { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string? Text { get; set; }

        [JsonProperty("schedule", NullValueHandling = NullValueHandling.Ignore)]
        public Schedule? Schedule { get; set; }

        [JsonProperty("agentId", NullValueHandling = NullValueHandling.Ignore)]
        public string? AgentId { get; set; }
    }

    public  class ChasitorIdleTimeout
    {
        [JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("warningTime")]
        public long WarningTime { get; set; }

        [JsonProperty("timeout")]
        public long Timeout { get; set; }
    }

    public  class GeoLocation
    {
        [JsonProperty("organization")]
        public string? Organization { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("countryName")]
        public string? CountryName { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("countryCode")]
        public string? CountryCode { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public  class Schedule
    {
        [JsonProperty("responseDelayMilliseconds")]
        public long ResponseDelayMilliseconds { get; set; }
    }

    public class Result
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("isAvailable")]
        public bool IsAvailable { get; set; }
    }

    public class LiveAgentSession
    {
        [JsonProperty("key")]
        public string? Key { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("clientPollTimeout")]
        public long ClientPollTimeout { get; set; }

        [JsonProperty("affinityToken")]
        public string? AffinityToken { get; set; }
    }
}
