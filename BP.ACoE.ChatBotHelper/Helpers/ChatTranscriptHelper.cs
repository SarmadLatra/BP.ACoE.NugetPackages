using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BP.ACoE.ChatBotHelper.Extensions
{
    public static class ChatTranscriptHelper
    {
        public static DateTime ConvertDateTime( DateTime userDateTime, string timeZone)
        {
            //Get the TimeZone of the user
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            //Convert to UTC
            DateTime convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(userDateTime, timeZoneInfo);
            return convertedDateTime;
        }

        public static string CalculateTimeSpan(DateTime FormattedTimeStamp, DateTime lastTime)
        {
            string addTicks;

            //Calculate Span
            TimeSpan span = FormattedTimeStamp.Subtract(lastTime);

            //Generate time
            if (span.Seconds <= 60 && span.Minutes == 0 && span.Hours == 0 && span.Days == 0)
            {
                addTicks = span.Seconds.ToString() + "s";
            }
            else if (span.Minutes <= 60 && span.Hours == 00 && span.Days == 0)
            {
                addTicks = span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }
            else if (span.Hours <= 24 && span.Days == 0)
            {
                addTicks = span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }
            else
            {
                addTicks = span.Days.ToString() + "d " + span.Hours.ToString() + "h " + span.Minutes.ToString() + "m " + span.Seconds.ToString() + "s";
            }

            return addTicks;
        }

        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString() + input.Substring(1).ToString().ToLower();
            }
        }
    }
}
