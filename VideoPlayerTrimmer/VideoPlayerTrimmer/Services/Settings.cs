using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace VideoPlayerTrimmer.Services
{
    public class Settings
    {
        public static int VideoBrightness
        {
            get { return Preferences.Get(nameof(VideoBrightness), 6); }
            set { Preferences.Set(nameof(VideoBrightness), value); }
        }


    }
}
