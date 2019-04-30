using System;
using System.Globalization;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Converters
{
    public class TimeSpanToVideoDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (TimeSpan)value;
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
