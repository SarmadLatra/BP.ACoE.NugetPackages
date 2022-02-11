using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Settings;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IAzureAuthService
    {
        Task<AccessTokenResponse?> GetAzureAuthToken(AzureAuthSettings azureAuthSettings);
    }
}
