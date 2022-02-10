namespace BP.ACoE.ChatBotHelper.Extensions
{
    public class MuleSoftException : HttpRequestException
    {
        public MuleSoftException(string responseData) : base(responseData)
        {

        }
    }

    public class MuleSoftCaseCreateException : MuleSoftException
    {
        public MuleSoftCaseCreateException(string responseData) : base(responseData)
        {
        }
    }

    public class MuleSoftCaseUpdateException : MuleSoftException
    {
        public MuleSoftCaseUpdateException(string responseData) : base(responseData)
        {
        }
    }

    public class MuleSoftCreateChatTranscriptException : MuleSoftException
    {
        public MuleSoftCreateChatTranscriptException(string responseData) : base(responseData)
        {
        }
    }

    public class MuleSoftUpdateChatTranscriptException : MuleSoftException
    {
        public MuleSoftUpdateChatTranscriptException(string responseData) : base(responseData)
        {
        }
    }

}
