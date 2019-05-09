using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Essentials;

namespace VideoPlayerTrimmer.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            CurrentVersion = VersionTracking.CurrentVersion;
            PreviousVersion = VersionTracking.PreviousVersion;
            AppName = AppInfo.Name;
#if DEBUG
            IsDebug = true;
#endif

        }

        public string CurrentVersion { get; set; }
        public string PreviousVersion { get; set; }
        public string AppName { get; set; }
        public bool IsDebug { get; set; } = false;

        public bool ResumePlayback
        {
            get => Settings.ResumePlayback;
            set => Settings.ResumePlayback = value;
        }

        public bool UseHardwareAcceleration
        {
            get => Settings.UseHardwareAcceleration;
            set => Settings.UseHardwareAcceleration = value;
        }
    }
}
