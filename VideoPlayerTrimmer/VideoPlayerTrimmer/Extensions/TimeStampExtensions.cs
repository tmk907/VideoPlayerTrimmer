using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Extensions
{
    public static class TimeStampExtensions
    {
        public static string ToVideoDuration(this TimeSpan duration)
        {
            if (duration.CompareTo(TimeSpan.Zero) == -1)
            {
                return "0:00";
            }
            string formatted = "";
            if (duration.Hours == 0)
            {
                if (duration.Duration().Minutes == 0) formatted = "0" + duration.ToString(@"\:ss");
                else formatted = duration.ToString(@"m\:ss");
            }
            else if (duration.Days == 0)
            {
                formatted = duration.ToString(@"h\:mm\:ss");
            }
            else
            {
                formatted = duration.ToString(@"d\.hh\:mm\:ss");
            }
            return formatted;
        }
    }
}
