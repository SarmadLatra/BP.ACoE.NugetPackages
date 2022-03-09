using System.Threading.Tasks;
using Microsoft.Graph;

namespace BPMeAUChatBot.API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendHtmlEmailAsync(Message mailMessage);
    }
}