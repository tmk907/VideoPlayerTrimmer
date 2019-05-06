using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Converters
{
    class TimeSpanToSecondsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var position = (TimeSpan)value;
            double offset = parameter == null ? 0 : 1;
            return position.TotalSeconds + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double seconds = (double)value;
            return TimeSpan.FromSeconds(seconds);
        }
    }
}
