using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using System.Collections.Concurrent;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ServicesUnitTests
{
    public class AppInsightsServiceUnitTest
    {
        [Fact]
        public async Task TestGetEntityByIdAzureTableStorageServiceAsync()
        {
            var mockLogger = new LoggerConfiguration().CreateLogger();

            TelemetryClient mockTelemetryClient = new MockTelemetryChannel().InitializeMockTelemetryChannel();

            var mockAppInsightsCustomEvents = new AppInsightsService(mockTelemetryClient, mockLogger);

            await mockAppInsightsCustomEvents.CustomTypedEventAsync
                (new AppInsightCustomEventTypes() { }, "mockCaseNumber");

            await mockAppInsightsCustomEvents.RaiseSaleForceEvent
                (new AppInsightCustomEventTypes(), "fakConversationId", "fakeMessage");

            await mockAppInsightsCustomEvents.RaiseGetTranscriptsEvent
                (new AppInsightCustomEventTypes(), "fakConversationId");

            Assert.NotNull(mockAppInsightsCustomEvents);
        }
    }
    public class MockTelemetryChannel : ITelemetryChannel
    {
        public ConcurrentBag<ITelemetry> SentTelemtries = new ConcurrentBag<ITelemetry>();
        public bool IsFlushed { get; private set; }
        public bool? DeveloperMode { get; set; }
        public string? EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            this.SentTelemtries.Add(item);
        }

        public void Flush()
        {
            this.IsFlushed = true;
        }

        public void Dispose()
        {

        }
        public TelemetryClient InitializeMockTelemetryChannel()
        {
            // Application Insights TelemetryClient doesn't have an interface (and is sealed)
            // Spin -up our own homebrew mock object
            MockTelemetryChannel mockTelemetryChannel = new MockTelemetryChannel();
            TelemetryConfiguration mockTelemetryConfig = new TelemetryConfiguration
            {
                TelemetryChannel = mockTelemetryChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };

            TelemetryClient mockTelemetryClient = new TelemetryClient(mockTelemetryConfig);
            return mockTelemetryClient;
        }
    }
}