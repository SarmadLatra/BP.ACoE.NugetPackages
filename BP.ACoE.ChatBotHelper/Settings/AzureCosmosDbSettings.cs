
namespace BP.ACoE.ChatBotHelper.Settings
{
    public class AzureCosmosDbSettings
    {

        public const string AzureCosmosSettingName = "CosmosDb";
        public string? HostUrl { get; set; }
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }
        public string? CosmosAccountKey { get; set; }
    }
}
