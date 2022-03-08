using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class GetChatTranscriptModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please provide the Conversation Id")]
        public string ConversationId { get; set; }
        public string ChatStartTime { get; set; }
        public string ChatEndTime { get; set; }
        public string ChatDuration { get; set; }
        public string Transcript { get; set; }
    }
}
