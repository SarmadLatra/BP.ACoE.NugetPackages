using BP.ACoE.ChatBotHelper.Extensions;
using Moq;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ExtensionsUnitTests
{
    public class ExtensionsUnitTest
    {
        [Fact]
        public void TestObjectToDictionaryExtension()
        {
            var obj = ObjectToDictionaryExtension.ToDictionary(new Mock<object>().Object);
            Assert.Equal(2, obj.Count);
        }

        [Fact]
        public async void TestExceptionObjectToDictionaryExtension()
        {
            _ = await Assert.ThrowsAsync<NullReferenceException>(() => (Task)ObjectToDictionaryExtension.ToDictionary(It.IsAny<string>()));
        }

        [Fact]
        public void TestJsonHelperExtension()
        {
            var obj = JsonHelper.ToJson(new { Id = "mock", Name = "mock" });
            Assert.IsType<string>(obj);
        }

        [Fact]
        public void MuleSoftException()
        {
            _ = Assert.IsType<MuleSoftException>(new MuleSoftException(It.IsAny<string>()));
        }

        [Fact]
        public void MuleSoftCaseCreateException()
        {
            _ = Assert.IsType<MuleSoftCaseCreateException>(new MuleSoftCaseCreateException(It.IsAny<string>()));
        }

        [Fact]
        public void MuleSoftCaseUpdateException()
        {
            _ = Assert.IsType<MuleSoftCaseUpdateException>(new MuleSoftCaseUpdateException(It.IsAny<string>()));
        }

        [Fact]
        public void MuleSoftCreateChatTranscriptException()
        {
            _ = Assert.IsType<MuleSoftCaseCreateException>(new MuleSoftCaseCreateException(It.IsAny<string>()));
        }

        [Fact]
        public void MuleSoftUpdateChatTranscriptException()
        {
            _ = Assert.IsType<MuleSoftUpdateChatTranscriptException>(new MuleSoftUpdateChatTranscriptException(It.IsAny<string>()));
        }
    }
}
