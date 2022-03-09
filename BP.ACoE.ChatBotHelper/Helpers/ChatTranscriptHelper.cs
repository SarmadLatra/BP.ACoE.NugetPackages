namespace BP.ACoE.ChatBotHelper.Helpers
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

        public static string CalculateTimeSpan(DateTime formattedTimeStamp, DateTime lastTime)
        {
            string addTicks;

            //Calculate Span
            var span = formattedTimeStamp.Subtract(lastTime);

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
            return input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString() + input[1..].ToLower()
            };
        }
    }
}
