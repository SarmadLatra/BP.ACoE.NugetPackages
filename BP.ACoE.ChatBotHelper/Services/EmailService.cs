using System.Net.Http.Headers;
using System.Threading.Tasks;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BPMeAUChatBot.API.Services.Interfaces;
using BPMeAUChatBot.API.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Serilog;

namespace BPMeAUChatBot.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAzureAuthService _authService;
        private readonly ILogger _logger;
        private readonly GraphApiAuthOptions _config;
        private AccessTokenResponse? _authResponse;

        public EmailService(IConfiguration configuration, ILogger logger, IOptions<GraphApiAuthOptions> options, IAzureAuthService authService)
        {
            _configuration = configuration;
            _authService = authService;
            _logger = logger.ForContext<EmailService>();
            this._config = options.Value;
        }

        public async Task SendHtmlEmailAsync(Message mailMessage)
        {
            await MakeAzureAuthResponse();
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", _authResponse?.AccessToken);
                return Task.CompletedTask;
            }));

            _logger.Information("just before sending the email");

            await graphClient.Users[_configuration.GetValue<string>("EmailFromAddress")]
                .SendMail(mailMessage, true)
                .Request()
                .PostAsync();

            _logger.Information("Email is sent");
        }

        private async Task MakeAzureAuthResponse()
        {
            if (_authResponse != null || string.IsNullOrEmpty(_authResponse?.AccessToken))
            {
                var config = new AzureAuthSettings()
                {
                    IsScope = true,
                    ClientId = _config.ClientId,
                    ClientSecret = _config.ClientSecret,
                    GrantType = "client_credentials",
                    //Tenant = _config.Tenant,
                    //Instance = _config.Instance,
                    Scope = $"{_config.ApiUrl}.default"
                };
                _authResponse = await _authService.GetAzureAuthToken(config);
            }
        }
    }
}
