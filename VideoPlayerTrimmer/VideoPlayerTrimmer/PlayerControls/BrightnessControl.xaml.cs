using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.PlayerControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BrightnessControl : ContentView
    {
        public BrightnessControl()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty BrightnessViewModelProperty =
            BindableProperty.Create(nameof(BrightnessViewModel),
                                    typeof(IBrightnessViewModel),
                                    typeof(PlaybackControls),
                                    default(IBrightnessViewModel));
        public IBrightnessViewModel BrightnessViewModel
        {
            get { return (IBrightnessViewModel)GetValue(BrightnessViewModelProperty); }
            set { SetValue(BrightnessViewModelProperty, value); }
        }

        private int initialBrightness;
        public double GridHeight = 15;

        public void BrightnessPanUpdated(PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    BrightnessViewModel.IsBrightnessIndicatorVisible = true;
                    initialBrightness = BrightnessViewModel.Brightness;
                    break;
                case GestureStatus.Running:
                    BrightnessViewModel.Brightness = CalculateValue(e.TotalY, initialBrightness, BrightnessViewModel.MaxBrightness);
                    BrightnessViewModel.ApplyBrightness();
                    break;
                case GestureStatus.Completed:
                    BrightnessViewModel.IsBrightnessIndicatorVisible = false;
                    break;
                case GestureStatus.Canceled:
                    BrightnessViewModel.IsBrightnessIndicatorVisible = false;
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