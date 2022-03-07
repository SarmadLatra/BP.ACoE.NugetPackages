using Newtonsoft.Json;

namespace BP.ACoE.ChatBotHelper.Models;

public abstract class BaseCosmosEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty(PropertyName = "txTimeStamp")]
    public DateTime TxTimestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty(PropertyName = "type")] public virtual string Type => nameof(BaseEntity);
}