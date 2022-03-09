using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class SalesForceAuthResponse
    {
        /// <summary>
        /// OAuth token that a connected app uses to request access to a protected resource on behalf of the client application. Additional permissions in the form of scopes can accompany the access token.
        /// </summary>
        [JsonProperty("access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// A Bearer token type, which is used for all responses that include an access token.
        /// </summary>
        [JsonProperty("token_type")]
        public string? TokenType { get; set; }

        /// <summary>
        /// A space-separated list of scopes values.
        /// </summary>
        [JsonProperty("scope")]
        public string? Scope { get; set; }

        /// <summary>
        /// A signed data structure that contains authenticated user attributes, including a unique identifier for the user and a time stamp indicating when the token was issued. It also identifies the requesting client app
        /// </summary>
        [JsonProperty("id_token")]
        public string? IdToken { get; set; }

        /// <summary>
        /// A URL indicating the instance of the user’s org. For example: https://yourInstance.salesforce.com/.
        /// </summary>
        [JsonProperty("instance_url")]
        public string? InstanceUrl { get; set; }
    }
}
