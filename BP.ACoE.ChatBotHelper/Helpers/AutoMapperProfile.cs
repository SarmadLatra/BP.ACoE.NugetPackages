using AutoMapper;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.ViewModels;

namespace BP.ACoE.ChatBotHelper.Helpers
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<BotConfigEntity, BotConfigViewModel>();
            CreateMap<CreateBotConfigViewModel, BotConfigEntity>();
        }
    }
}
