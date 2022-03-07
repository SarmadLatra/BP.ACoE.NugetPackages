using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BPMeAUChatBot.API.ViewModels
{
    public class WebFormDataModel
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ConversationId { get; set; }

        /// <summary>
        /// expected values
        /// 1. HANDOFF_FAILED  (hand off failed no web form filled by customer)
        /// 2. HANDOFF_FAILURE_WEBFORM_SUBMIT (hand off failed and web form filled by customer)
        /// 3. WEBFORM_SUBMIT (a new web form is filled)
        /// </summary>
        [Required]
        public string Status { get; set; }

        public string Description { get; set; }

        public string UserQuery { get; set; }

        public string UserIntent { get; set; }

        public string CaseId { get; set; }

        public string Type { get; set; }

        public string LovType { get; set; }

        public string LovArea { get; set; }

        public string LovSubArea { get; set; }

        public string Comments { get; set; }

        public string RewardsNumber { get; set; }
        
        public string QantasNumber { get; set; }
        
        public string ImageUrl { get; set; }

        public string ImageType { get; set; }

        public string TransactionDate { get; set; }
    }
}
