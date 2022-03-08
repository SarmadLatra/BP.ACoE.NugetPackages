using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class ChatTranscriptModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide the User Id")]
        public string UserId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide the Conversation Id")]
        public string ConversationId { get; set; }

        public string CaseId { get; set; }

        public string QueueName { get; set; }

        public string ChannelName { get; set; }

        [Required]
        public string ChatBody { get; set; }

        public string ipAddress { get; set; }

        [JsonProperty("id")]
        public string TranscriptId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("errors")]
        public string[] Error { get; set; }


    }
}