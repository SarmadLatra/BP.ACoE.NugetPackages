using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Services.Interfaces;
using BPMeAUChatBot.API.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Serilog;
using System.Net.Http.Headers;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class EmailService : IEmailService
    {
        private readonly IAzureAuthService _authService;
        private readonly ILogger _logger;
        private readonly GraphApiAuthSettings _config;
        private AccessTokenResponse? _authResponse;

        public EmailService(ILogger logger, IOptions<GraphApiAuthSettings> options, IAzureAuthService authService)
        {
            _authService = authService;
            _logger = logger.ForContext<EmailService>();
            this._config = options.Value;
        }

        public async Task SendHtmlEmailAsync(Message mailMessage)
        {
            if (_authResponse != null || string.IsNullOrEmpty(_authResponse?.AccessToken))
                _authResponse = await _authService.GetAzureAuthToken(new AzureAuthSettings()
                {
                    IsScope = true,
                    ClientId = _config.ClientId,
                    ClientSecret = _config.ClientSecret,
                    GrantType = "client_credentials",
                    Scope = $"{_config.ApiUrl}.default"
                });

            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", _authResponse?.AccessToken);
                return Task.CompletedTask;
            }));

            _logger.Information("just before sending the email");

            await graphClient.Users[mailMessage.From.EmailAddress.Address]
                .SendMail(mailMessage, true)
                .Request()
                .PostAsync();

            _logger.Information("Email is sent");
        }
    }
}
