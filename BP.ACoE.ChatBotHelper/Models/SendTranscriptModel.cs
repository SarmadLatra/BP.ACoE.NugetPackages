using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class SendTranscriptModel
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string ConversationId { get; set; }

        public string CaseId { get; set; }
    }
}
