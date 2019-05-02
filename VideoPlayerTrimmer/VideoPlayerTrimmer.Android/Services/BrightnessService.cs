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

[assembly: Dependency(typeof(BrightnessService))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class BrightnessService : IBrightnessService
    {
        private float? originalBrightness;

        public float GetBrightness()
        {
            var window = CrossCurrentActivity.Current.Activity.Window;
            var attributesWindow = new WindowManagerLayoutParams();

            attributesWindow.CopyFrom(window.Attributes);
            var b = attributesWindow.ScreenBrightness;
            return b;
        }

        public void RestoreBrightness()
        {
            if (originalBrightness.HasValue)
            {
                SetBrightness(originalBrightness.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="brightness">From 0 to 1</param>
        public void SetBrightness(float brightness)
        {
            if (brightness > 1f || brightness == 0f) return;
            if(!originalBrightness.HasValue)
            {
                originalBrightness = GetBrightness();
            }
            var window = CrossCurrentActivity.Current.Activity.Window;
            var attributesWindow = new WindowManagerLayoutParams();
            attributesWindow.CopyFrom(window.Attributes);
            attributesWindow.ScreenBrightness = brightness;
            window.Attributes = attributesWindow;
        }
    }
}