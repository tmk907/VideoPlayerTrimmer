namespace VideoPlayerTrimmer.PlayerControls
{
    public interface IBrightnessViewModel
    {
        int Brightness { get; set; }
        bool IsBrightnessIndicatorVisible { get; set; }
        int MaxBrightness { get; }

        void ApplyBrightness();
        void RestoreBrightness();
    }
}