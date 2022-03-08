using System.Threading.Tasks;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.ViewModels;
namespace BPMeAUChatBot.API.Services.Interfaces
{
    public interface IChatTransactionService
    {
        Task<ChatbotTransactionEntity> GetTransactionByConversationId(string conversationId, string userId = "");
        Task<ChatbotTransactionEntity> GetTransactionByRowKey(string rowKey);
        Task<ChatbotTransactionEntity> UpdateTransactionAsync(ChatbotTransactionEntity txEntity);
    }
}