using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.ViewModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace BPMeAUChatBot.API.Services.Interfaces
{
    public interface IChatTranscriptService
    {
        Task<IList<Activity>> GetChatTranscriptFromStore(string conversationId, ITranscriptStore store,
            string channelId = "directline");

        Task<int> GetChatBotTurnCountFromTranscriptAsync(string conversationId, ITranscriptStore makeBlobsTranscriptStore);
        Task<GetChatTranscriptModel> GetChatTranscriptAsync(GetChatTranscriptModel model, ITranscriptStore makeBlobsTranscriptStore);
        // Task<(XWPFDocument document, string fileName)> GetChatTranscriptForCustomerInDocAsync(string conversationId, Func<ITranscriptStore> makeBlobsTranscriptStore);

        Task<(byte[] document, string fileName)> GetChatTranscriptForCustomerInPDFAsync(string conversationId, ITranscriptStore makeBlobsTranscriptStore);
        Task<string> GetFirstIntentAsync(string conversationId, ITranscriptStore makeBlobsTranscriptStore);

        // new service methods

        Task<ChatTranscript> GetChatTranscriptEntity(string userId, string conversationId);
        Task<ChatTranscript> UpdateChatTranscriptEntity(ChatTranscript sendTranscriptEntity);
        Task<bool> SendChatTranscriptAsync(ChatTranscript sendTranscriptEntity);
        Task<bool> SendChatTranscriptAsync(ChatBotSeibelEntity sendTranscriptEntity);
        Task<bool> ProcessChatTranscriptEntityAsync(string userId, string conversationId, string caseId = "");
        Task<bool> SendChatTranscriptAsync(string refId);
        Task<ChatTranscriptDto> GetChatTranscriptText(string conversationId);
        Task<ChatTranscript> RequestChatTranscriptEntityAsync(SendTranscriptModel model);
    }
}
