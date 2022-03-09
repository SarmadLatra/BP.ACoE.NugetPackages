using BP.ACoE.ChatBotHelper.ViewModels;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IBotConfigService
    {
        Task<BotConfigViewModel> FindBotConfigWithKeyAsync(BotConfigQueryViewModel model);
        Task<BotConfigViewModel> SaveBotConfig(CreateBotConfigViewModel model);
    }
}