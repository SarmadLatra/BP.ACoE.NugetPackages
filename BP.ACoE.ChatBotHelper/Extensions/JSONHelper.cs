using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Extensions
{
    public static class JsonHelper
    {
        public static string ToJson(this object data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}
