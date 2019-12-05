using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.PlayerControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VolumeControl : ContentView
    {
        public VolumeControl()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty VolumeViewModelProperty =
            BindableProperty.Create(nameof(VolumeViewModel),
                            typeof(IVolumeViewModel),
                            typeof(PlaybackControls),
                            default(IVolumeViewModel));
        public IVolumeViewModel VolumeViewModel
        {
            get { return (IVolumeViewModel)GetValue(VolumeViewModelProperty); }
            set { SetValue(VolumeViewModelProperty, value); }
        }

        private int initialVolume;
        public double GridHeight = 15;

        public void VolumePanUpdated(PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    initialVolume = VolumeViewModel.Volume;
                    break;
                case GestureStatus.Running:
                    VolumeViewModel.IsVolumeIndicatorVisible = true;
                    VolumeViewModel.Volume = CalculateValue(e.TotalY, initialVolume, VolumeViewModel.MaxVolume);
                    VolumeViewModel.ApplyVolume();
                    break;
                case GestureStatus.Completed:
                    VolumeViewModel.IsVolumeIndicatorVisible = false;
                    break;
                case GestureStatus.Canceled:
                    VolumeViewModel.IsVolumeIndicatorVisible = false;
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

            double maxChange = GridHeight;
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