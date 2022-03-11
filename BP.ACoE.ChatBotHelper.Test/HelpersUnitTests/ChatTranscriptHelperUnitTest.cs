using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Settings;
using Moq;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.HelpersUnitTests
{
    public class ChatTranscriptHelperUnitTest
    {
        [Fact]
        public void TestChatTranscriptHelper()
        {
            var convertedDateTime = ChatTranscriptHelper.ConvertDateTime(It.IsAny<DateTime>(), "GMT Standard Time");

            var calculatedTimeSpan = ChatTranscriptHelper.CalculateTimeSpan(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            var firstCharToUpper = ChatTranscriptHelper.FirstCharToUpper("mockInput");

            Assert.IsType<DateTime>(convertedDateTime);
            Assert.IsType<string>(calculatedTimeSpan);
            Assert.Equal("mockinput", firstCharToUpper);
        }
    }
}
