using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.RangeSlider.Forms;

namespace VideoPlayerTrimmer.Views
{
    public class VideoPlayerContentPage : BaseContentPage<VideoPlayerViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPlayerPage : VideoPlayerContentPage
    {
        public VideoPlayerPage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void MediaPlayerChanged(object sender, LibVLCSharp.Shared.MediaPlayerChangedEventArgs e)
        {
            if (e.NewMediaPlayer != null)
            {
                ViewModel.StartPlayingOrResume();
            }
        }

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            ViewModel.SeekTo(e.NewValue);
        }

        private int initialVolume;
        private int initialBrightness;

        private void VolumePanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    ViewModel.IsVolumeIndicatorVisible = true;
                    initialVolume = ViewModel.Volume;
                    break;
                case GestureStatus.Running:
                    ViewModel.Volume = CalculateValue(e.TotalY, initialVolume, ViewModel.MaxVolume);
                    ViewModel.ApplyVolume();
                    break;
                case GestureStatus.Completed:
                    ViewModel.IsVolumeIndicatorVisible = false;
                    break;
                case GestureStatus.Canceled:
                    ViewModel.IsVolumeIndicatorVisible = false;
                    break;
            }
        }

        private void BrightnessPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    ViewModel.IsBrightnessIndicatorVisible = true;
                    initialBrightness = ViewModel.Brightness;
                    break;
                case GestureStatus.Running:
                    ViewModel.Brightness = CalculateValue(e.TotalY, initialBrightness, ViewModel.MaxBrightness);
                    ViewModel.ApplyBrightness();
                    break;
                case GestureStatus.Completed:
                    ViewModel.IsBrightnessIndicatorVisible = false;
                    break;
                case GestureStatus.Canceled:
                    ViewModel.IsBrightnessIndicatorVisible = false;
                    break;
            }
        }

        private int CalculateValue(double totalChange, int value, int maxValue)
        {
            double minimumChange = 30;
            bool decrease = totalChange > 0;
            totalChange = Math.Abs(totalChange);
            if (totalChange < minimumChange) return value;
            totalChange -= minimumChange;

            double maxChange = VolumeGrid.Height;
            double maxValueChange = 15;
            double heightPerValue = (maxChange - minimumChange) / maxValueChange;

            int valueChange = (int)Math.Round(totalChange / heightPerValue);
            if (decrease) valueChange *= -1;
            value += valueChange;

            App.DebugLog(String.Format("{0} {1} {2}", totalChange, heightPerValue, valueChange));
            return Math.Max(0, Math.Min(value, maxValue));
        }
    }
}