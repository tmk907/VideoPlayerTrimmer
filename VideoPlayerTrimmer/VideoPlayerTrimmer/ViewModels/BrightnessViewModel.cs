using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.PlayerControls;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class BrightnessViewModel : BindableBase, IBrightnessViewModel
    {
        private readonly IBrightnessService _brightnessController;

        public BrightnessViewModel(IBrightnessService brightnessController)
        {
            _brightnessController = brightnessController;
            Brightness = Settings.VideoBrightness;
        }

        private int brightness;
        public int Brightness
        {
            get { return brightness; }
            set
            {
                SetProperty(ref brightness, value);
                Settings.VideoBrightness = value;
            }
        }

        public int MaxBrightness { get; } = 15;

        public void ApplyBrightness()
        {
            App.DebugLog(brightness.ToString());
            _brightnessController.SetBrightness(((float)brightness) / (float)MaxBrightness);
        }

        public void RestoreBrightness()
        {
            _brightnessController.RestoreBrightness();
        }

        private bool isBrightnessIndicatorVisible = false;
        public bool IsBrightnessIndicatorVisible
        {
            get { return isBrightnessIndicatorVisible; }
            set { SetProperty(ref isBrightnessIndicatorVisible, value); }
        }
    }
}
