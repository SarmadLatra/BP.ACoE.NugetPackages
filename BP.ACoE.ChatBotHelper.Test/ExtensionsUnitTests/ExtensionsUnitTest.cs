using BP.ACoE.ChatBotHelper.Extensions;
using BP.ACoE.ChatBotHelper.Helpers;
using BP.ACoE.ChatBotHelper.Models;
using BP.ACoE.ChatBotHelper.Services;
using BP.ACoE.ChatBotHelper.Services.Interfaces;
using BP.ACoE.ChatBotHelper.Settings;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BP.ACoE.ChatBotHelper.Test.ExtensionsUnitTest
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
