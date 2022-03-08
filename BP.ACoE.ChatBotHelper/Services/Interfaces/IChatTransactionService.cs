using BP.ACoE.ChatBotHelper.Models;
using System.Threading.Tasks;
namespace BPMeAUChatBot.API.Services.Interfaces
{
    public interface IChatTransactionService
    {
        Task<ChatbotTransactionEntity> GetTransactionByConversationId(string conversationId, string userId = "");
        Task<ChatbotTransactionEntity> GetTransactionByRowKey(string rowKey);
        Task<ChatbotTransactionEntity> UpdateTransactionAsync(ChatbotTransactionEntity txEntity);
    }
}