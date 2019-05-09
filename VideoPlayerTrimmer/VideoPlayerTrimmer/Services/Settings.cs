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

        public static bool ResumePlayback
        {
            get { return Preferences.Get(nameof(ResumePlayback), true); }
            set { Preferences.Set(nameof(ResumePlayback), value); }
        }

        public static bool UseHardwareAcceleration
        {
            get { return Preferences.Get(nameof(UseHardwareAcceleration), false); }
            set { Preferences.Set(nameof(UseHardwareAcceleration), value); }
        }
    }
}
