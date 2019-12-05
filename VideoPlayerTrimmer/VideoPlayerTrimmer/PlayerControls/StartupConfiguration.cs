using System;
using System.Collections.Generic;
using System.Linq;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class StartupConfiguration
    {
        public string FilePath { get; set; }

        public bool AutoPlay { get; set; } = true;
        public long ResumeTime { get; set; } = 0;

        public TimeSpan EmbeddedSubtitlesDelay { get; set; } = TimeSpan.Zero;
        public string EmbeddedSubtitlesEncoding { get; set; } = LibVlcOptions.GetSubtitleEncoding();
        public List<SubitlesConfig> ExternalSubtitles { get; } = new List<SubitlesConfig>();
        public int SelectedSubtitlesSpu { get; set; } = -1;

        public SubitlesConfig SelectedSubtitlesFile => ExternalSubtitles.FirstOrDefault(x => x.IsSelected);
        public bool IsEmbeddedSubtitlesSelected => SelectedSubtitlesSpu != -1;
        public bool IsFileSubtitlesSelected => SelectedSubtitlesFile != null;
    }

    public class SubitlesConfig
    {
        public string FileUrl { get; set; } = "";
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
        public string Encoding { get; set; } = LibVlcOptions.Encoding.UTF8;
        public string EncodingOption => LibVlcOptions.GetSubtitleEncoding(Encoding);

        public bool IsSelected { get; set; }
    }
}
