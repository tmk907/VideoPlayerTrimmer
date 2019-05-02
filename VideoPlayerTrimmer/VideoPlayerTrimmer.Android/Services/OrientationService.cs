using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(OrientationService))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class OrientationService : IOrientationService
    {
        public void ChangeToLandscape()
        {
            CrossCurrentActivity.Current.Activity.RequestedOrientation = ScreenOrientation.SensorLandscape;
        }

        public void RestoreOrientation()
        {
            CrossCurrentActivity.Current.Activity.RequestedOrientation = ScreenOrientation.User;
        }
    }
}