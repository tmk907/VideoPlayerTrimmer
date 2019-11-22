using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class VlcSubtitles : BindableBase
    {
        public VlcSubtitles(int spu, string name)
        {
            Name = name;
            Spu = spu;
        }

        public string Name { get; set; }
        
        public int Spu { get; set; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
    }
}
