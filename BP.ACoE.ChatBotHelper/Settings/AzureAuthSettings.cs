
namespace BP.ACoE.ChatBotHelper.Settings
{
    public class AzureAuthSettings
    {
        public string? GrantType { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public bool IsScope { get; set; }
        public string? Scope { get; set; }
        public string? Resource { get; set; }
        public string? AuthAuthUrl { get; set; }
    }
}
