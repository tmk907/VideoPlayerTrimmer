using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.MediaHelpers
{
    public class SubtitleInfo : BindableBase
    {
        public string Name { get; set; }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
        
        public int VlcId { get; set; }

        public string FilePath { get; set; }

        public bool IsExternal { get { return !string.IsNullOrEmpty(FilePath); } }
    }
}
