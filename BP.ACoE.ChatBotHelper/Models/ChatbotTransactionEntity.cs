using BP.ACoE.ChatBotHelper.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BPMeAUChatBot.API.Models
{
    public class ChatbotTransactionEntity: BaseEntity
    {
        // RowKey, Partiton Key, Timestamp 

        [Required]
        public string UserId { get; set; }
        [Required]
        public string ConversationId { get; set; }
        public bool ChatStarted { get; set; }
        public int BotTransactionCount { get; set; }
        public string Status { get; set; }
        public DateTime TxTimestamp { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string salesforceId { get; set; }

    }
}
