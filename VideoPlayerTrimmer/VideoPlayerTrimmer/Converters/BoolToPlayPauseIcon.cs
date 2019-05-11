using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Converters
{
    class BoolToPlayPauseIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlaying = (bool)value;
            if (isPlaying)
            {
                return "ep-controller-paus";
            }
            else
            {
                return "ep-controller-play";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
