using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Helpers
{
    public static class UserHelper
    {
        public static string ToJson<T>(this T anyObject)
        {
            var json = JsonConvert.SerializeObject(anyObject);
            return json;
        }
        public static string StripUserId(this string userId)
        {
            var parts = userId.Split("_");
            return parts.Length > 1 ? parts[1] : parts[0];
        }
    }
}
