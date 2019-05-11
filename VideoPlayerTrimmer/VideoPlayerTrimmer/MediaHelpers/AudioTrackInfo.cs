using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.MediaHelpers
{
    public class AudioTrackInfo : BindableBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
    }
}
