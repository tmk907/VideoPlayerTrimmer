using System;
using System.Globalization;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Converters
{
    class BoolToStringConverter : IValueConverter
    {
        public string IfFalse { get; set; }
        public string IfTrue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return IfTrue;
            }
            else
            {
                return IfFalse;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
