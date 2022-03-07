using System;
using System.ComponentModel.DataAnnotations;
using BP.ACoE.ChatBotHelper.Models;

namespace BPMeAUChatBot.API.Models
{
    public class ChatBotSeibelEntity : BaseEntity
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ConversationId { get; set; }
        public string Type { get; set; }
        public string Comments { get; set; }
        public string UserIntent { get; set; }
        public string UserQuery { get; set; }
        public DateTime TxTimestamp { get; set; }
        public string Status { get; set; }
        public string EmailStatus { get; set; }
        public string StationName { get; set; }
        public string IssueType { get; set; }
        public string NewEmail { get; set; }
        public string OldEmail { get; set; }
    }
}
