using Microsoft.Graph;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendHtmlEmailAsync(Message mailMessage);
    }
}