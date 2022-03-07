using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.Services.Interfaces;
using BPMeAUChatBot.API.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Table;
using Serilog;

namespace BPMeAUChatBot.API.Services
{
    public class ChatTransactionService : IChatTransactionService
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly ChatTransactionSettings _chatTransactionSettings;
        private readonly string _tableName;
        private readonly string _partitionKey;
        private const string ClassName = "ChatTransactionService--";
        private const string _jobQueue = "txqueue";
        public ChatTransactionService(IOptions<ChatTransactionSettings> chatTransactionSettings, ILogger logger, IStorageService storageService)
        {
            _chatTransactionSettings = chatTransactionSettings.Value;
            _logger = logger.ForContext<ChatTransactionService>();
            _storageService = storageService;
            _tableName = _chatTransactionSettings.ChatTxTable;
            _partitionKey = _chatTransactionSettings.PartitionKey;
        }
        public async Task<ChatbotTransactionEntity> GetTransactionByConversationId(string conversationId, string userId = "")
        {
            const string methodName = "GetMuleSoftCaseData";
            _logger.Information($"{ClassName}-{methodName} Processing started");

            var partitionFilter =
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey);
            var conversationIdCondition = TableQuery.GenerateFilterCondition($"{nameof(WebFormDataModel.ConversationId)}",
                QueryComparisons.Equal, conversationId);

            var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, conversationIdCondition);

            if (!string.IsNullOrEmpty(userId))
            {
                var userIdCondition = TableQuery.GenerateFilterCondition($"{nameof(WebFormDataModel.UserId)}",
                    QueryComparisons.Equal, userId);
                finalFilter = TableQuery.CombineFilters(finalFilter, TableOperators.And, userIdCondition);
            }

            //_logger.Information($"{ClassName}-{methodName} query built");
            //var query = new TableQuery<ChatbotTransactionEntity>().Where(finalFilter);
            var data = (await _storageService.GetEntitiesByQuery<ChatbotTransactionEntity>(_tableName, finalFilter)).ToList();
            _logger.Information($"{ClassName}-{methodName} data received");
            if (!data.Any())
            {
                throw new HttpRequestException("Conversation not found");
            }

            return data.FirstOrDefault();
        }
    }
}
