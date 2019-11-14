using System;
using System.Globalization;
using VideoPlayerTrimmer.Extensions;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class TimeSpanToVideoDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (TimeSpan)value;
            return duration.ToVideoDuration();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
