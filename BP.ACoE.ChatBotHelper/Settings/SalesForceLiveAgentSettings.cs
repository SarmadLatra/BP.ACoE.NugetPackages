namespace BP.ACoE.ChatBotHelper.Settings
{
    public class SalesForceLiveAgentSettings
    {
        public string? ChatEndUrl { get; set; }
        public string? AvailableLiveAgentsUrl { get; set; }
        public string? SendChatMessageUrl { get; set; }
        public string? GetChatMessagesUrl { get; set; }
        public string? StartChatUrl { get; set; }
        public string? LiveAgentSessionUrl { get; set; }
        public string? LiveAgentApiVersion { get; set; }
        public string? OrganizationId { get; set; }
        public string? DeploymentId { get; set; }
        public string? ButtonId { get; set; }
        public bool BuildAvailableLiveAgentUrl { get; set; } = true;
    }
}
