namespace VideoPlayerTrimmer.PlayerControls
{
    public interface IVolumeViewModel
    {
        bool IsVolumeIndicatorVisible { get; set; }
        int MaxVolume { get; set; }
        int Volume { get; set; }

        void ApplyVolume();
    }
}