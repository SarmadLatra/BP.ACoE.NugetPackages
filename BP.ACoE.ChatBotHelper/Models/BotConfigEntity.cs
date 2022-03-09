using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Models
{
    public class BotConfigEntity: BaseEntity
    {
        [Required]
        public string? ConfigKey { get; set; }
        [Required]
        public string? ConfigValue { get; set; }
        public string? Description { get; set; }
        public ConfigValueTypes ValueType { get; set; } = ConfigValueTypes.String;
    }

    public enum ConfigValueTypes
    {
        String,
        Number,
        Decimal,
        Date,
        DateTime,
        Boolean,
    }
}
