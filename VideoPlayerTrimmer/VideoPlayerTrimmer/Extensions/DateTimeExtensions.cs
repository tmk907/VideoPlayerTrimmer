using System;

namespace VideoPlayerTrimmer.Extensions
{
    public static class TimeStampExtenstions
    {
        public static DateTime DateTimeFromUnixTimestamp(this long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }
    }
}
