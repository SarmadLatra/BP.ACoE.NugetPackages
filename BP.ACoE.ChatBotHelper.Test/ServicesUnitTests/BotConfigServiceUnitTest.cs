using AutoMapper;
using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using BP.ACoE.ChatBotHelper.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using Serilog;
using System.Linq.Expressions;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class BotConfigServiceUnitTest
    {
        private static IMapper? _mapper;

        public BotConfigServiceUnitTest()
        {
            if (_mapper == null)
            {
                var mappingConfig = new MapperConfiguration(mc =>
                {
                    mc.AddProfile(new AutoMapperProfile());
                });
                IMapper mapper = mappingConfig.CreateMapper();
                _mapper = mapper;
            }
        }

        [Fact]
        public async Task Test_BotConfigService()
        {
            var botConfigService = setParam("BotConfigService");

            var saveBotConfig = await botConfigService.SaveBotConfig(new ViewModels.CreateBotConfigViewModel()
            {
                ConfigKey = "mockConfigKey",
                ConfigValue = "mockConfigValue",
                Description = "mockDescription"
            });

            _ = await Assert.ThrowsAsync<HttpRequestException>(() => botConfigService.FindBotConfigWithKeyAsync(new BotConfigQueryViewModel()
            {
                ConfigKey = "mockConfigKey"
            }));

        }

        [Fact]
        public async Task Test_FindBotConfigWithKeyWithException()
        {
            var botConfigService = setParam("FindBotConfigWithKeyException");

            _ = await Assert.ThrowsAsync<HttpRequestException>(() => botConfigService.FindBotConfigWithKeyAsync(new BotConfigQueryViewModel()
            {
                ConfigKey = "mockConfigKey"
            }));

        }
        [Fact]
        public async Task Test_FindBotConfigWithKeyWithResponse()
        {
            var botConfigResponseService = setParam("FindBotConfigWithKeyResponse");

            var result = await botConfigResponseService.FindBotConfigWithKeyAsync(new BotConfigQueryViewModel()
            {
                ConfigKey = "mockConfigKey"
            });
            Assert.Equal("mockConfigKey", result.ConfigKey);
        }

        public BotConfigService setParam(string config)
        {
            var configurations = new Mock<IConfiguration>();
            var logger = new LoggerConfiguration().CreateLogger();
            var mockStorageService = new Mock<IStorageService>();
            var mockStorageTableSettings = new Mock<IOptions<StorageTableSettings>>();

            var botConfigEntity = new BotConfigEntity()
            {
                DateCreated = new DateTime(),
                ETag = new Azure.ETag(),
                PartitionKey = "mockPartitionKey",
                RowKey = "mockRowKey",
                Timestamp = new DateTimeOffset(),
                ConfigKey = "mockConfigKey",
                ConfigValue = "mockConfigValue",
                Description = "mockDesc",
                ValueType = new ConfigValueTypes()
            };

            mockStorageTableSettings.Setup(x => x.Value).Returns(new StorageTableSettings()
            {
                PartitionKey = "mockPartitionKey",
                TableName = "mockTableName"
            });

            mockStorageService.Setup(x => x.InsertEntity<BotConfigEntity>(It.IsAny<string>(), It.IsAny<BotConfigEntity>()))
                .ReturnsAsync(botConfigEntity);

            if (!config.Equals("BotConfigService"))
            {
                var listBotConfig = new Mock<List<BotConfigEntity>>();
                listBotConfig.Object.Add(botConfigEntity);

                Expression<Func<BotConfigEntity, bool>> mockExpression = binding => (true);
                if (config.Equals("FindBotConfigWithKeyResponse"))
                    mockStorageService.Setup(x => x.GetEntitiesByQuery<BotConfigEntity>(It.IsAny<string>(),
                    It.Is<Expression<Func<BotConfigEntity, bool>>>(criteria => true)))
                    .ReturnsAsync(listBotConfig.Object);
                else
                    mockStorageService.Setup(x => x.GetEntitiesByQuery<BotConfigEntity>(It.IsAny<string>(),
                    It.Is<Expression<Func<BotConfigEntity, bool>>>(criteria => false)))
                    .ReturnsAsync(listBotConfig.Object);
            }
            return _ = new BotConfigService(logger, mockStorageService.Object,
                mockStorageTableSettings.Object, _mapper);
        }
    }
}
