using System.ComponentModel.DataAnnotations;
using BP.ACoE.ChatBotHelper.Models;

namespace BP.ACoE.ChatBotHelper.ViewModels
{
    public class BotConfigViewModel
    {
        public string? RowKey { get; set; }
        public string? ConfigKey { get; set; }
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
        public ConfigValueTypes ValueType { get; set; }
        public DateTime TxTimestamp { get; set; }
    }

    public class CreateBotConfigViewModel
    {
        public string? ConfigKey { get; set; }
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
    }

    public class BotConfigQueryViewModel
    {
        [Required]
        public string? ConfigKey { get; set; }

    }
}
