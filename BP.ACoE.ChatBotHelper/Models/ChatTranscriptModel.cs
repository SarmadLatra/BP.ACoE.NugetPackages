using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMeAUChatBot.API.ViewModels
{
    public class ChatTranscriptDto
    {
        public string StartTimeFormatted { get; set; }
        public string EndTimeFormatted { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string TranscriptText { get; set; }

        public string ChatDuration { get; set; }

    }

    public class ChatTranscriptDocumentDto
    {
        public string StartTimeFormatted { get; set; }
        public string EndTimeFormatted { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public string ChatDuration { get; set; }

        public (byte[], string) TranscriptDocument { get; set; }
    }
}
