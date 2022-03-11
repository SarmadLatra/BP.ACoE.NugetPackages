using AutoMapper;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BP.ACoE.ChatBotHelper.ViewModels;
using Microsoft.Extensions.Options;
using Serilog;

namespace BP.ACoE.ChatBotHelper.Services
{
    public class BotConfigService : IBotConfigService
    {
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        private readonly StorageTableSettings _storageSettings;
        private readonly IMapper _autoMapper;
        private const string ClassName = "BotConfigService--";
        public BotConfigService(ILogger logger, IStorageService storageService, IOptions<StorageTableSettings> storageOptions, IMapper autoMapper)
        {
            _storageService = storageService;
            _storageSettings = storageOptions.Value;
            _autoMapper = autoMapper;
            _logger = logger.ForContext<BotConfigService>();
        }

        public async Task<BotConfigViewModel> SaveBotConfig(CreateBotConfigViewModel model)
        {
            const string methodName = "SaveBotConfig---";
            _logger.Information($"{ClassName}-{methodName} Processing started");

            var newEntity = _autoMapper.Map<BotConfigEntity>(model);
            newEntity.PartitionKey = _storageSettings.PartitionKey;
            newEntity.RowKey = Guid.NewGuid().ToString();
            newEntity.DateCreated = DateTime.UtcNow;
            newEntity.ValueType = ConfigValueTypes.String;
            var entity = await _storageService.InsertEntity<BotConfigEntity>(_storageSettings.TableName, newEntity);
            _logger.Information($"{ClassName}-{methodName} bot config key saved");
            return _autoMapper.Map<BotConfigViewModel>(entity);
        }

        public async Task<BotConfigViewModel> FindBotConfigWithKeyAsync(BotConfigQueryViewModel model)
        {
            const string methodName = "FindBotConfigWithKeyAsync---";

            _logger.Information($"{ClassName}-{methodName} Processing started");


            var data = (await _storageService.GetEntitiesByQuery<BotConfigEntity>(_storageSettings.TableName, x => x.PartitionKey == _storageSettings.PartitionKey && x.ConfigKey == model.ConfigKey)).ToList();
            _logger.Information($"{ClassName}-{methodName} data received");
            if (!data.Any())
            {
                throw new HttpRequestException("Config Key not found");
            }
            _logger.Information($"{ClassName}-{methodName} bot config key info received");

            var botConfig = _autoMapper.Map<BotConfigViewModel>(data.FirstOrDefault());
            return botConfig;
        }
    }
}
