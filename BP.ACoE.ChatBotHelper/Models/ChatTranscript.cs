using BP.ACoE.ChatBotHelper.Models;

namespace BPMeAUChatBot.API.Models
{
    public class ChatTranscript : BaseEntity
    {
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string CaseId { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string TxTimestamp { get; set; }
    }
}