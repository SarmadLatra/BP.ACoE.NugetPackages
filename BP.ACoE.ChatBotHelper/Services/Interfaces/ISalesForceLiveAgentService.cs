using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.Services.Interfaces
{
    public interface ISalesForceLiveAgentService
    {
        Task<string> EndLiveAgentChatSession(ChatEndModel model);
        Task<string> SendChatMessageToAgent(ChatMessageModel model);
        Task<LiveAgentMessageResponse> GetCurrentMessagesFromLiveAgent(string sessionId, string affinityToken);
        Task<string> StartChatWithLiveAgent(StartChatModel model);
        Task<LiveAgentSession> GetLiveAgentSessions();
        Task<LiveAgentResponse?> GetAvailableLiveAgents();
        Task SendTypingEventToAgent(string sessionKey, string affinityToken);
        Task SendNotTypingEventToAgent(string sessionKey, string affinityToken);
    }
}
