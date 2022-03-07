﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure;
using BPMeAUChatBot.API.Models;
using BPMeAUChatBot.API.Services.Interfaces;
using BPMeAUChatBot.API.ViewModels;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using Serilog;
using System.Text.RegularExpressions;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using Microsoft.WindowsAzure.Storage.Table;
using BPMeAUChatBot.API.Helpers;

namespace BPMeAUChatBot.API.Services
{
    public class ChatTranscriptService : IChatTranscriptService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpClient _client;
        private readonly IStorageService _storageService;
        private readonly IEncryptionService _decryptionService;
        private readonly IEmailService _emailService;
        private readonly IChatTransactionService _chatTransactionService;
        private readonly ITranscriptStore _transcriptStore;
        private const string ClassName = "ChatTranscriptService ---";
        private readonly string _tableName;
        private readonly string _partitionKey;

        public ChatTranscriptService(IConfiguration configuration, ILogger logger, HttpClient client,
            IStorageService storageService, IEncryptionService decryption, IEmailService emailService, IChatTransactionService chatTransactionService, ITranscriptStore transcriptStore)
        {
            _configuration = configuration;
            _logger = logger.ForContext<ChatTranscriptService>();
            _client = client;
            _storageService = storageService;
            _decryptionService = decryption;
            _emailService = emailService;
            _tableName = _configuration["SendTranscriptTable"];
            _partitionKey = _configuration["PartitionKey"];
            _chatTransactionService = chatTransactionService;
            _transcriptStore = transcriptStore;
        }


        public ITranscriptStore GetBlobsTranscriptStore()
        {
            return _transcriptStore;
        }

        public async Task<List<IActivity>> GetChatTranscriptFromStore(string conversationId, ITranscriptStore store, string channelId = "directline")
        {
            string pageToken = null;
            var transcript = new List<IActivity>();
            do
            {
                var pagedTranscript = await store.GetTranscriptActivitiesAsync(channelId, conversationId,
                    continuationToken: pageToken);
                transcript.AddRange(pagedTranscript.Items);
                pageToken = pagedTranscript.ContinuationToken;
            }
            while (pageToken != null);
            return transcript;

        }

        public async Task<int> GetChatBotTurnCountFromTranscriptAsync(string conversationId, Func<ITranscriptStore> makeBlobsTranscriptStore)
        {
            var store = makeBlobsTranscriptStore();
            var transcript = await GetChatTranscriptFromStore(conversationId, store);
            var botName = _configuration.GetValue<string>("ChatbotName");
            const string methodName = "GetChatBotTurnCountFromTranscriptAsync---";
            _logger.Information($"{ClassName}{methodName} -- Started");

            var turnCount = 0;
            if (transcript != null && transcript.Count > 0)
            {
                turnCount = transcript
                    .Count(x => x.Type == ActivityTypes.Message && x.From.Name != null && x.From.Name.Contains(botName.ToLower()));
            }

            return turnCount;
        }



        private async Task<string> LoadEmailTemplate(ChatBotSeibelEntity seibelEntity)
        {
            var templatePath = "";
            if (seibelEntity.Type == "FORM_REWARDS" || seibelEntity.IssueType == "REWARDS_RELATED")
            {
                templatePath = _configuration.GetValue<string>("RewardEmailTemplatePath");


            }
            else if (seibelEntity.Type == "STATION_ISSUE_FORM" || seibelEntity.IssueType == "STATION_RELATED")
            {
                templatePath = _configuration.GetValue<string>("StationEmailTemplatePath");


            }
            else if (seibelEntity.Type == "FLEET_FORM")
            {
                templatePath = _configuration.GetValue<string>("GeneralFleetEmailTemplatePath");


            }
            else if (seibelEntity.IssueType == "FLEET_RELATED")
            {
                templatePath = _configuration.GetValue<string>("FleetEmailTemplatePath");


            }
            else
            {
                templatePath = _configuration.GetValue<string>("CustomerSupportEmailTemplatePath");

            }
            if (!System.IO.File.Exists(templatePath)) throw new Exception("Email template was not found.");
            var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
            return emailTemplate;
        }

        private async Task<(byte[] document, string filename)> GenerateChatTranscriptPDF(IList<IActivity> transcript, string conversationId)
        {
            const string methodName = "SaveChatTranscriptForCustomerAsync---";
            StringBuilder transciptFilename = new StringBuilder("BPRewardsVirtualAssistant");

            _logger.Information($"{ClassName}{methodName} -- Started");

            string userName = null;
            string botText = null;

            DateTimeOffset? timestamp = null;
            DateTime submitDateTime = default;
            var chatCount = 0;

            var UserInf = await _chatTransactionService.GetTransactionByConversationId(conversationId);
            var customerName = UserInf.Name;
            var customerEmail = _decryptionService.Decrypt(UserInf.Email);
            transciptFilename.Append($"-{customerName}");

            List<ChatTextComponent> pdfContents = new List<ChatTextComponent>();
            StringBuilder contentItem = new StringBuilder();
            var filteredTranscript = transcript.Where(i => i.Type == ActivityTypes.Message
                                                        && string.IsNullOrEmpty(i.AsMessageActivity().From.Name) == false
                                                        && string.IsNullOrEmpty(i.AsMessageActivity().Text) == false);
            filteredTranscript = filteredTranscript.GroupBy(x => x.AsMessageActivity().Text).Select(x => x.First());

            //finalize the name of PDF based on timestamp of first message
            if (filteredTranscript.FirstOrDefault() != null)
            {
                timestamp = filteredTranscript.FirstOrDefault().Timestamp;
                submitDateTime = ConvertDateTime(timestamp.Value.UtcDateTime);
                transciptFilename.Append($"-{submitDateTime.ToString("MM-dd-yyyy")}");
            }

            foreach (var message in filteredTranscript)
            {
                userName = message.AsMessageActivity().From.Name;
                botText = message.AsMessageActivity().Text;
                chatCount++;
                if (chatCount == 1)
                {
                    //Add default text before the transcript begins.
                    pdfContents.Add(new ChatTextComponent(string.Empty, "BP Rewards Virtual Assistant Transcript", ChatTextComponentProperties.None));
                    pdfContents.Add(new ChatTextComponent("Customer Name", customerName, ChatTextComponentProperties.BoldMessage));
                    pdfContents.Add(new ChatTextComponent("Customer Email", customerEmail, ChatTextComponentProperties.None));

                    //We need 2 empty rows between the customer email and welcome message saved via bot, hence adding following lines.
                    pdfContents.Add(new ChatTextComponent(" ", " ", ChatTextComponentProperties.None));
                    pdfContents.Add(new ChatTextComponent(" ", " ", ChatTextComponentProperties.None));
                }
                if (message.From?.Name?.ToLower().Contains("tstraubotsvc") == true)
                {
                    if (message.AsMessageActivity().Text?.Contains(":") == true && !message.AsMessageActivity().Text.Contains("http"))
                    {
                        message.From.Name = message.AsMessageActivity().Text.Split(":")[0];
                        message.AsMessageActivity().Text = message.AsMessageActivity().Text.Split(":")[1];
                    }
                    else
                    {
                        message.AsMessageActivity().From.Name = "Virtual Assistant";
                    }
                }
                pdfContents.Add(new ChatTextComponent(message.AsMessageActivity().From.Name,
                                                        message.AsMessageActivity().Text,
                                                        string.Equals(customerName, message.AsMessageActivity().From.Name, StringComparison.OrdinalIgnoreCase)
                                                            ? ChatTextComponentProperties.BoldUserName
                                                            : ChatTextComponentProperties.None
                                                        ));

            }

            var dirPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var headerImageFullName = dirPath + _configuration.GetValue<string>("ChatTranscriptPDFHeaderImagePath");
            var footerImageFullName = dirPath + _configuration.GetValue<string>("ChatTranscriptPDFFooterImagePath");

            byte[] fileContents = PDFHelper.GeneratePdf(headerImageFullName, footerImageFullName, pdfContents);
            _logger.Information($"{ClassName}{methodName} -- Ended");

            return (document: fileContents, filename: transciptFilename.ToString());
        }

        private async Task<ChatTranscriptDocumentDto> LoadChatTranscriptAsync(List<IActivity> transcript, string ConversationId, string UserId)
        {
            const string methodName = "LoadChatTranscriptAsync---";
            _logger.Information($"{ClassName}{methodName} -- Started");

            var doc = await GenerateChatTranscriptPDF(transcript, ConversationId);

            _logger.Information($"{ClassName}{methodName} -- Ended");

            var chatStartTime = transcript.FirstOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;
            var chatEndTime = transcript.LastOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;

            var timeZone = _configuration.GetValue<string>("TimeZone");
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            var chatStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatStartTime.Value.UtcDateTime, timeZoneInfo);
            var chatEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatEndTime.Value.UtcDateTime, timeZoneInfo);

            var chatDuration = chatEndDateTime.Subtract(chatStartDateTime).ToString();

            var transcriptDto = new ChatTranscriptDocumentDto
            {
                TranscriptDocument = doc,
                StartTimeFormatted = chatStartDateTime.ToString(CultureInfo.InvariantCulture),
                EndTimeFormatted = chatEndDateTime.ToString(CultureInfo.InvariantCulture),
                StartTime = chatStartDateTime,
                EndTime = chatEndDateTime,
                ChatDuration = chatDuration
            };

            return transcriptDto;
        }

        public async Task<string> GetFirstIntentAsync(string conversationId, Func<ITranscriptStore> makeBlobsTranscriptStore)
        {
            const string methodName = "GetFirstIntent---";
            _logger.Information($"{ClassName}{methodName} -- Started");
            var defaultMessage = "Customer has not selected or typed anything.";
            var transcript = await GetChatTranscriptFromStore(conversationId, makeBlobsTranscriptStore());
            if (transcript.Count < 5)
            {
                return defaultMessage;
            }
            // TODO: exclude bot name properly
            try
            {
                var firstIntent = transcript.FirstOrDefault(x => x.Type == ActivityTypes.Message
                                                                 && !x.From.Name.ToLower().Contains("bpmeau"))
                    ?.AsMessageActivity();
                _logger.Information(JsonConvert.SerializeObject(transcript.Where(x => x.Type == ActivityTypes.Message)));
                if (firstIntent == null) return defaultMessage;
                if (string.IsNullOrEmpty(firstIntent.Text))
                {
                    dynamic obj = firstIntent.Value;
                    _logger.Information(obj["action"].ToString());
                    defaultMessage = $"{obj["action"]} {obj["type"]}";
                }
                else
                {
                    defaultMessage = firstIntent.Text;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{ClassName}{methodName} {e.Message}");
            }

            return defaultMessage;
        }


        public async Task<bool> SendChatTranscriptAsync(ChatBotSeibelEntity sendTranscriptEntity)
        {

            var toEmailId = GetEmailId(sendTranscriptEntity);

            var transcript =
                await GetChatTranscriptFromStore(sendTranscriptEntity.ConversationId, this.GetBlobsTranscriptStore());
            if (!transcript.Any()) return false;
            //  load chat transcript in document format
            // transcript is there process email 
            var transcriptDocDto = new ChatTranscriptDocumentDto();

            transcriptDocDto = await LoadChatTranscriptAsync(transcript, sendTranscriptEntity.ConversationId, sendTranscriptEntity.UserId);
            //   load email template 
            var emailTemplate = await LoadEmailTemplate(sendTranscriptEntity);

            var userInf = await _chatTransactionService.GetTransactionByConversationId(sendTranscriptEntity.ConversationId);
            var firstintent = await GetFirstIntentAsync(sendTranscriptEntity.ConversationId, this.GetBlobsTranscriptStore);

            emailTemplate = emailTemplate.Replace("$vChatStartTime$", transcriptDocDto.StartTimeFormatted)
                .Replace("$vChatEndTime$", transcriptDocDto.EndTimeFormatted)
                .Replace("$vChatDuration$", transcriptDocDto.ChatDuration).Replace("$vCustomerName$", userInf.Name)
                .Replace("$vComments$", firstintent);
            var customerId = userInf.UserId.StripUserId();
            //  1. create mail message 
            //  2. use email service to send the email message with attachment
            var message = new Message
            {
                ToRecipients =
                    new List<Recipient>()
                    {
                        // todo: toEmailId update 
                        new Recipient {EmailAddress = new EmailAddress() {Address = toEmailId},},
                    },
                From = new Recipient()
                {
                    EmailAddress = new EmailAddress()
                    {
                        Name = _configuration.GetValue<string>("ChatbotName"),
                        Address = _configuration.GetValue<string>("EmailFromAddress")
                    }
                },

                Body = new ItemBody() { ContentType = Microsoft.Graph.BodyType.Html, Content = emailTemplate },
                HasAttachments = true,
                Subject = (_configuration.GetValue<bool>("TestEnvironment") ? "Test-" : "") + "BP Rewards Virtual Assistant Chat Transcript-" + customerId
            };

            _ = bool.TryParse(_configuration.GetValue<string>("SendCCEmail"), out bool isSendCCEmail);
            if (isSendCCEmail)
            {

                message.BccRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress() {Address = _configuration.GetValue<string>("EmailCCAddress")},
                    },
                    new Recipient
                    {
                        EmailAddress = new EmailAddress() {Address =  _decryptionService.Decrypt(userInf.Email)},
                    }
                };
            }
            //var date = userInf.Timestamp.ToString("yyyy-MM-dd");
            var date = userInf.Timestamp.ToString();

            var dateparts = date?.Split("-");
            if (transcriptDocDto.TranscriptDocument.Item1 != null)
            {
                message.Attachments = new MessageAttachmentsCollectionPage()
                {
                    new FileAttachment()
                    {
                        Name = "BPRewardsVirtualAssistant-"+userInf.Name+"-"+dateparts?[1]+"-"+dateparts?[2]+"-"+dateparts?[0]+".pdf",
                        ContentType = "application/pdf",
                        ContentBytes =transcriptDocDto.TranscriptDocument.Item1,
                    }
                };
            }

            await _emailService.SendHtmlEmailAsync(message);
            return true;

        }

        public async Task<bool> SendChatTranscriptAsync(ChatTranscript sendTranscriptEntity)
        {

            {
                var transcript =
                    await GetChatTranscriptFromStore(sendTranscriptEntity.ConversationId, this.GetBlobsTranscriptStore());

                string fullname = " ";

                if (!transcript.Any()) return false;
                //  load chat transcript in document format
                // transcript is there process email 
                var transcriptDocDto = await LoadChatTranscriptAsync(transcript, sendTranscriptEntity.ConversationId, sendTranscriptEntity.UserId);
                //   load email template 
                var emailTemplate = await LoadEmailTemplate();

                var userInf = await _chatTransactionService.GetTransactionByConversationId(sendTranscriptEntity.ConversationId);
                var firstintent = await GetFirstIntentAsync(sendTranscriptEntity.ConversationId, this.GetBlobsTranscriptStore); ;
                var name = userInf.Name.Split(" ");

                //Pascal Case
                foreach (var i in name)
                {
                    fullname += FirstCharToUpper(i) + " ";
                }

                emailTemplate = emailTemplate.Replace("$vChatStartTime$", transcriptDocDto.StartTimeFormatted)
                    .Replace("$vChatEndTime$", transcriptDocDto.EndTimeFormatted)
                    .Replace("$vChatDuration$", transcriptDocDto.ChatDuration).Replace("$vCustomerName$", fullname.Trim())
                    .Replace("$vComments$", firstintent);

                var customerId = userInf.UserId.StripUserId();
                //  1. create mail message 
                //  2. use email service to send the email message with attachment
                var message = new Message
                {
                    ToRecipients =
                        new List<Recipient>()
                        {
                        new Recipient {EmailAddress = new EmailAddress() {Address = sendTranscriptEntity.Email},},
                        },
                    From = new Recipient()
                    {
                        EmailAddress = new EmailAddress()
                        {
                            Name = _configuration["ChatbotName"],
                            Address = _configuration["EmailFromAddress"]
                        }
                    },

                    Body = new ItemBody() { ContentType = Microsoft.Graph.BodyType.Html, Content = emailTemplate },
                    HasAttachments = true,
                    Subject = (_configuration.GetValue<bool>("TestEnvironment") ? "Test-" : "") + "BP Rewards Virtual Assistant Chat Transcript-" + customerId

                };

                _ = bool.TryParse(_configuration.GetValue<string>("SendCCEmail"), out bool isSendCCEmail);
                if (isSendCCEmail)
                {

                    message.BccRecipients = new List<Recipient>()
                {
                    new Recipient
                    {
                        EmailAddress = new EmailAddress() {Address = _configuration.GetValue<string>("EmailCCAddress")},
                    }
                };
                }
                //var date = userInf.Timestamp.ToString("yyyy-MM-dd");
                var date = userInf.Timestamp.ToString();

                var dateparts = date?.Split("-");
                if (transcriptDocDto.TranscriptDocument.Item1 != null)
                {
                    message.Attachments = new MessageAttachmentsCollectionPage()
                {
                    new FileAttachment()
                    {
                         Name = "BPRewardsVirtualAssistant-"+userInf.Name+"-"+dateparts?[1]+"-"+dateparts?[2]+"-"+dateparts?[0]+".pdf",
                        ContentType = "application/pdf",
                        ContentBytes = transcriptDocDto.TranscriptDocument.Item1,
                    }
                };
                }

                await _emailService.SendHtmlEmailAsync(message);
                return true;

            }
        }

        #region Private Methods
        private DateTime ConvertDateTime(DateTime userDateTime)
        {
            const string methodName = "ConvertDateTime--";
            _logger.Information($"{ClassName}{methodName} is called");

            //Get the TimeZone of the user
            var timeZone = _configuration.GetValue<string>("TimeZone");
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            //Convert to UTC
            DateTime convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(userDateTime, timeZoneInfo);
            return convertedDateTime;
        }


        private string GetEmailId(ChatBotSeibelEntity sendTranscriptEntity)
        {
            string key;
            if (sendTranscriptEntity.Type == "GENERAL_FORM")
            {
                key = sendTranscriptEntity.IssueType + "ToEmail";
            }
            else
            {
                key = sendTranscriptEntity.Type + "ToEmail";
            }
            return _configuration[key.ToString()];
        }

        async Task<IList<Activity>> IChatTranscriptService.GetChatTranscriptFromStore(string conversationId, ITranscriptStore store, string channelId)
        {
            var results = new List<IActivity>();
            string pageToken = String.Empty;

            do
            {
                var transcripts = await store.GetTranscriptActivitiesAsync(channelId,
                conversationId, continuationToken: pageToken);
                pageToken = transcripts.ContinuationToken;
                results.AddRange(transcripts.Items);

            } while (pageToken != null);

            return results.ConvertAll(o => (Activity)o);
        }
        [Obsolete]
        public async Task<GetChatTranscriptModel> GetChatTranscriptAsync(GetChatTranscriptModel model, Func<ITranscriptStore> makeBlobsTranscriptStore)
        {

            {
                var transcript = await GetChatTranscriptFromStore(model.ConversationId, makeBlobsTranscriptStore());

                const string methodName = "GetChatTranscript---";
                _logger.Information($"{ClassName}{methodName} -- Started");
                var lastTime = DateTime.UtcNow;
                var runOnce = true;
                var chatTranscript = new StringBuilder();

                foreach (var message in transcript.Where(i => i.Type == ActivityTypes.Message))
                {
                    if (string.IsNullOrEmpty(message.AsMessageActivity().From.Name) ||
                        string.IsNullOrEmpty(message.AsMessageActivity().Text)) continue;

                    var timestamp = message.Timestamp.ToString();
                    //Convert to DateTime
                    var formattedTimeStamp = DateTime.Parse(timestamp ?? String.Empty);

                    if (runOnce)
                    {
                        lastTime = formattedTimeStamp;
                        runOnce = false;
                    }

                    //Calculate Span
                    _logger.Information($"{ClassName}{methodName} called CalculateTimeSpan");
                    var addTicks = CalculateTimeSpan(formattedTimeStamp, lastTime);
                    string findColonInMessage;
                    if (message.From?.Name?.ToLower().Contains(_configuration["ChatbotName"]) == true)
                    {
                        findColonInMessage = message.AsMessageActivity().Text.Length > 25
                            ? message.AsMessageActivity().Text?.Substring(0, 25)
                            : findColonInMessage = message.AsMessageActivity().Text;

                        chatTranscript.Append(findColonInMessage?.Contains(":") == true
                            ? $"({addTicks}) {message.AsMessageActivity().Text}<br><br>"
                            : $"({addTicks}) {_configuration["ChatBotTranscriptName"]}: {message.AsMessageActivity().Text}<br><br>");
                    }
                    else
                    {
                        if (message.AsMessageActivity().Text.Contains(message.AsMessageActivity().From.Name))
                        {
                            chatTranscript.Append($"({addTicks}) {message.AsMessageActivity().Text}<br><br>");
                        }
                        else
                        {
                            chatTranscript.Append($"({addTicks}) {message.AsMessageActivity().From.Name}: {message.AsMessageActivity().Text}<br><br>");
                        }
                    }
                }

                var chatTranscriptResponse = new GetChatTranscriptModel
                {
                    Transcript = chatTranscript.ToString()
                };
                _logger.Information($"{ClassName}{methodName} -- {chatTranscriptResponse.Transcript}");
                _logger.Information($"{ClassName}{methodName} -- Ended");
                return chatTranscriptResponse;
            }
        }

        // public async Task<(XWPFDocument document, string fileName)> GetChatTranscriptForCustomerInDocAsync(string conversationId, Func<ITranscriptStore> makeBlobsTranscriptStore)
        // {
        //     const string methodName = "GetChatTranscriptForCustomerAsync---";
        //     _logger.Information($"{ClassName}{methodName} -- Started");

        //     var store = makeBlobsTranscriptStore();
        //     var transcript = await GetChatTranscriptFromStore(conversationId, store);


        //     var doc = await GenerateChatTranscriptDocument(transcript, conversationId);
        //     //_logger.Information($"{ClassName}{methodName} -- {out1}");
        //     _logger.Information($"{ClassName}{methodName} -- Ended");
        //     return doc;
        // }

        public async Task<(byte[] document, string fileName)> GetChatTranscriptForCustomerInPDFAsync(string conversationId, Func<ITranscriptStore> makeBlobsTranscriptStore)
        {
            const string methodName = "GetChatTranscriptForCustomerAsync---";
            _logger.Information($"{ClassName}{methodName} -- Started");

            var store = makeBlobsTranscriptStore();
            var transcript = await GetChatTranscriptFromStore(conversationId, store);

            var doc = await GenerateChatTranscriptPDF(transcript, conversationId);
            //_logger.Information($"{ClassName}{methodName} -- {out1}");
            _logger.Information($"{ClassName}{methodName} -- Ended");
            return doc;
        }

        public async Task<ChatTranscript> GetChatTranscriptEntity(string userId, string conversationId)
        {

            {
                const string methodName = "GetMuleSoftCaseData";
                _logger.Information($"{ClassName}-{methodName} Processing started");

                var partitionFilter =
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey);
                var userIdCondition = TableQuery.GenerateFilterCondition($"{nameof(ChatTranscriptModel.UserId)}",
                    QueryComparisons.Equal, userId);
                var conversationCondition = TableQuery.GenerateFilterCondition($"{nameof(ChatTranscriptModel.ConversationId)}",
                    QueryComparisons.Equal, conversationId);

                var finalFilter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, userIdCondition);
                finalFilter = TableQuery.CombineFilters(finalFilter, TableOperators.And, conversationCondition);

                var data = (await _storageService.GetEntitiesByQuery<ChatTranscript>(_tableName, finalFilter)).ToList();
                _logger.Information($"{ClassName}-{methodName} data received");
                if (!data.Any()) throw new HttpRequestException("Chat Transcript record was not found");
                return data[0];
            }
        }
        private static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString() + input.Substring(1).ToString().ToLower();
            }
        }

        public async Task<ChatTranscript> UpdateChatTranscriptEntity(ChatTranscript sendTranscriptEntity)
        {
            var updatedEntity = await _storageService.UpdateEntity(_tableName, sendTranscriptEntity);
            return updatedEntity;
        }

        public async Task<bool> ProcessChatTranscriptEntityAsync(string userId, string conversationId, string caseId = "")

        {
            const string methodName = "ProcessChatTranscriptEntityAsync--";
            _logger.Error($"{ClassName}{methodName} Email send process started");

            try
            {
                var sendTranscriptEntity = await this.GetChatTranscriptEntity(userId, conversationId);
                _logger.Error($"{ClassName}{methodName} Chat transcript found");
                sendTranscriptEntity.CaseId = caseId;
                if (sendTranscriptEntity.Status == "NEW")
                {
                    var emailSendStatus = await this.SendChatTranscriptAsync(sendTranscriptEntity);
                    if (emailSendStatus)
                    {
                        sendTranscriptEntity.Status = "EMAIL_SENT";

                        sendTranscriptEntity.Email = string.Empty;
                        sendTranscriptEntity =
                            await this.UpdateChatTranscriptEntity(sendTranscriptEntity);

                        _logger.Debug($"{ClassName}{methodName} Updated email send status");
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"{ClassName}{methodName}{e.Message}", e);

            }
            return false;
        }

        public async Task<bool> SendChatTranscriptAsync(string refId)

        {
            var sendTranscriptEntity = await _storageService.GetEntityByRowKey<ChatTranscript>(_tableName, refId);
            if (sendTranscriptEntity != null)
            {
                return await this.SendChatTranscriptAsync(sendTranscriptEntity);
            }

            return false;

        }

        public async Task<ChatTranscriptDto> GetChatTranscriptText(string conversationId)
        {
            var transcript =
                await GetChatTranscriptFromStore(conversationId, this.GetBlobsTranscriptStore());
            return !transcript.Any() ? throw new DataException("No chat record was found") : LoadChatTranscriptTextAsync(transcript);
        }

        public async Task<ChatTranscript> RequestChatTranscriptEntityAsync(SendTranscriptModel model)
        {
            var userInfo = await _chatTransactionService.GetTransactionByConversationId(model.ConversationId, model.UserId);

            var entity = new ChatTranscript()
            {
                PartitionKey = _partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                ConversationId = model.ConversationId,
                UserId = model.UserId,
                Status = "NEW",
                TxTimestamp = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                CaseId = model.CaseId,
                Email = _decryptionService.Decrypt(userInfo.Email)
            };

            var insertResult = await _storageService.InsertEntity(_tableName, entity);
            return insertResult;
        }

        #endregion
        #region Private Methods
        private ChatTranscriptDto LoadChatTranscriptTextAsync(IEnumerable<IActivity> transcript)
        {
            const string methodName = "LoadChatTranscriptTextAsync---";
            _logger.Information($"{ClassName}{methodName} -- Started");

            var chatStartTime = transcript.FirstOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;
            var chatEndTime = transcript.LastOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;

            var timeZone = _configuration.GetValue<string>("TimeZone");
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            var chatStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatStartTime.Value.UtcDateTime, timeZoneInfo);
            var chatEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatEndTime.Value.UtcDateTime, timeZoneInfo);

            var chatDuration = chatEndDateTime.Subtract(chatStartDateTime).ToString();

            var chatTranscript = new StringBuilder();

            foreach (var message in transcript.Where(i => i.Type == ActivityTypes.Message))
            {
                if (string.IsNullOrEmpty(message.AsMessageActivity().From.Name) ||
                    string.IsNullOrEmpty(message.AsMessageActivity().Text)) continue;

                var timestamp = message.Timestamp.ToString();
                //Convert to DateTime
                var formattedTimeStamp = DateTime.Parse(timestamp);

                //Calculate Span
                _logger.Information($"{ClassName}{methodName} called CalculateTimeSpan");
                var addTicks = CalculateTimeSpan(formattedTimeStamp, chatStartDateTime);
                string findColonInMessage;
                if (message.From?.Name?.ToLower().Contains(_configuration["ChatbotName"]) == true)
                {
                    findColonInMessage = message.AsMessageActivity().Text.Length > 25
                        ? message.AsMessageActivity().Text?.Substring(0, 25)
                        : findColonInMessage = message.AsMessageActivity().Text;

                    chatTranscript.Append(findColonInMessage?.Contains(":") == true
                        ? $"({addTicks}) {message.AsMessageActivity().Text}<br><br>"
                        : $"({addTicks}) {_configuration["ChatBotTranscriptName"]}: {message.AsMessageActivity().Text}<br><br>");
                }
                else
                {
                    if (message.AsMessageActivity().Text.Contains(message.AsMessageActivity().From.Name))
                    {
                        chatTranscript.Append($"({addTicks}) {message.AsMessageActivity().Text}<br><br>");
                    }
                    else
                    {
                        chatTranscript.Append($"({addTicks}) {message.AsMessageActivity().From.Name}: {message.AsMessageActivity().Text}<br><br>");
                    }
                }
            }

            _logger.Information($"{ClassName}{methodName} -- Ended");

            var transcriptDto = new ChatTranscriptDto
            {
                TranscriptText = chatTranscript.ToString(),
                StartTimeFormatted = chatStartDateTime.ToString(CultureInfo.InvariantCulture),
                EndTimeFormatted = chatEndDateTime.ToString(CultureInfo.InvariantCulture),
                StartTime = chatStartDateTime,
                EndTime = chatEndDateTime,
                ChatDuration = chatDuration
            };

            return transcriptDto;
        }


        private async Task<ChatTranscriptDocumentDto> LoadChatTranscriptAsync(IList<IActivity> transcript, string ConversationId, string UserId)
        {
            const string methodName = "LoadChatTranscriptAsync---";
            _logger.Information($"{ClassName}{methodName} -- Started");

            var doc = await GenerateChatTranscriptPDF(transcript, ConversationId);

            _logger.Information($"{ClassName}{methodName} -- Ended");

            var chatStartTime = transcript.FirstOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;
            var chatEndTime = transcript.LastOrDefault(x => x.Type == ActivityTypes.Message).Timestamp;

            var timeZone = _configuration.GetValue<string>("TimeZone");
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            var chatStartDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatStartTime.Value.UtcDateTime, timeZoneInfo);
            var chatEndDateTime = TimeZoneInfo.ConvertTimeFromUtc(chatEndTime.Value.UtcDateTime, timeZoneInfo);

            var chatDuration = chatEndDateTime.Subtract(chatStartDateTime).ToString();

            var transcriptDto = new ChatTranscriptDocumentDto
            {
                TranscriptDocument = doc,
                StartTimeFormatted = chatStartDateTime.ToString(CultureInfo.InvariantCulture),
                EndTimeFormatted = chatEndDateTime.ToString(CultureInfo.InvariantCulture),
                StartTime = chatStartDateTime,
                EndTime = chatEndDateTime,
                ChatDuration = chatDuration
            };

            return transcriptDto;
        }

        private async Task<string> LoadEmailTemplate()
        {
            var templatePath = _configuration.GetValue<string>("EmailTemplatePath");
            if (!System.IO.File.Exists(templatePath)) throw new Exception("Email template was not found.");
            var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
            return emailTemplate;
        }


        private string CalculateTimeSpan(DateTime FormattedTimeStamp, DateTime lastTime)
        {
            string addTicks;

            //Calculate Span
            TimeSpan span = FormattedTimeStamp.Subtract(lastTime);

            //Generate time
            if (span.Seconds <= 60 && span.Minutes == 0 && span.Hours == 0 && span.Days == 0)
            {
                addTicks = span.Seconds.ToString() + "s";
            }
            else if (span.Minutes <= 60 && span.Hours == 00 && span.Days == 0)
            {
                addTicks = span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }
            else if (span.Hours <= 24 && span.Days == 0)
            {
                addTicks = span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }
            else
            {
                addTicks = span.Days.ToString() + "d " + span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }

            return addTicks;
        }


        #endregion
    }
}