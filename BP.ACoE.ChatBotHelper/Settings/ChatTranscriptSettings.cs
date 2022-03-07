using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Settings
{
    public class ChatTranscriptSettings
    {
        public string? ChatbotName { get; set; }
        public string SendTranscriptTable { get; set; }
        public string PartitionKey { get; set; }
        public string? RewardEmailTemplatePath { get; set; }
        public string? StationEmailTemplatePath { get; set; }
        public string? GeneralFleetEmailTemplatePath { get; set; }
        public string? FleetEmailTemplatePath { get; set; }
        public string? CustomerSupportEmailTemplatePath { get; set; }
        public string? ChatTranscriptPDFHeaderImagePath { get; set; }
        public string? ChatTranscriptPDFFooterImagePath { get; set; }
        public string? EmailFromAddress { get; set; }
        public bool TestEnvironment { get; set; } = true;
        public string? SendCCEmail { get; set; }
        public string? EmailCCAddress { get; set; }
        public string? ChatBotTranscriptName { get; set; }
        public string? EmailTemplatePath { get; set; }
        public string? TimeZone { get; set; }
        public string? FLEET_FORMToEmail { get; set; }
        public string? REWARD_FORMToEmail { get; set; }
        public string? PAYMENT_FORMToEmail { get; set; }
        public string? APP_TECH_FORMToEmail { get; set; }
        public string? CHANGE_EMAIL_FORMToEmail { get; set; }
        public string? CLOSE_ACCOUNT_FORMToEmail { get; set; }
        public string? STATION_ISSUE_FORMToEmail { get; set; }
        public string? CLICKCOLLECT_RELATEDToEmail { get; set; }
        public string? ACCOUNT_RELATEDToEmail { get; set; }
        public string? PAYMENT_RELATEDToEmail { get; set; }
        public string? REWARDS_RELATEDToEmail { get; set; }
        public string? STATION_RELATEDToEmail { get; set; }
        public string? APP_RELATEDToEmail { get; set; }
        public string? OTHER_QUERYToEmail { get; set; }
        public string? FLEET_RELATEDToEmail { get; set; }

    }
}
