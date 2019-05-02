using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using VideoPlayerTrimmer.Droid.BroadcastReceivers;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(VolumeService))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class VolumeService : IVolumeService
    {
        private readonly AudioManager audioManager;
        public VolumeService()
        {
            audioManager = (AudioManager)CrossCurrentActivity.Current.Activity.GetSystemService(Context.AudioService);
            MessagingCenter.Subscribe<MainActivity>(this, MainActivity.VolumeChangeMessage, (sender) =>
            {
                VolumeChanged?.Invoke(this, new VolumeChangedEventArgs(0));
            });
        }

        public event EventHandler<VolumeChangedEventArgs> VolumeChanged;

        public int GetMaxVolume()
        {
            int maxVolume = audioManager.GetStreamMaxVolume(Stream.Music);
            return maxVolume;
        }

        public int GetVolume()
        {
            int curVolume = audioManager.GetStreamVolume(Stream.Music);
            return curVolume;
        }

        public void SetVolume(int volume)
        {
            audioManager.SetStreamVolume(Stream.Music, volume, VolumeNotificationFlags.RemoveSoundAndVibrate);
        }
    }
}