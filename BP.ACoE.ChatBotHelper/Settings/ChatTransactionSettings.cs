using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Settings
{
    public class ChatTransactionSettings
    {
        public string ChatTxTable { get; set; }
        public string PartitionKey { get; set; }
    }
}
