using System.Collections.Generic;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.MediaHelpers
{
    public class MediaInfo : BindableBase
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string VideoTitle { get; set; }
        public float Fps { get; set; }
        public List<AudioTrackInfo> AudioTracks { get; set; }
        public List<SubtitleInfo> Subtitles { get; set; }
        public string Resolution { get; set; }

    }
}
