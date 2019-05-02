using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Plugin.CurrentActivity;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(StatusBarService))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class StatusBarService : IStatusBarService
    {
        public bool IsVisible
        {
            get
            {
                var window = CrossCurrentActivity.Current.Activity.Window;
                return window.DecorView.SystemUiVisibility == StatusBarVisibility.Visible;
            }
            set
            {
                var window = CrossCurrentActivity.Current.Activity.Window;
                if (value)
                {
                    window.ClearFlags(WindowManagerFlags.Fullscreen);
                    window.ClearFlags(WindowManagerFlags.KeepScreenOn);
                    window.AddFlags(WindowManagerFlags.ForceNotFullscreen);
                }
                else
                {
                    window.AddFlags(WindowManagerFlags.Fullscreen);
                    window.AddFlags(WindowManagerFlags.KeepScreenOn);
                }
                //window.DecorView.SystemUiVisibility = value ? StatusBarVisibility.Visible : StatusBarVisibility.Hidden; 
            }
        }
    }
}