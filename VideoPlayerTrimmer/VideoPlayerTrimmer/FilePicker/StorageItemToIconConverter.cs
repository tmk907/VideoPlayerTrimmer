using System;
using System.Globalization;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.FilePicker
{
    public class StorageItemToIconConverter : IValueConverter
    {
        public string FolderIcon { get; set; }
        public string FileIcon { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is FileItem)
            {
                return FileIcon;
            }
            else if(value is FolderItem)
            {
                return FolderIcon;
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
