using System;
using System.Collections.Generic;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class StartupConfiguration
    {
        public string FilePath { get; set; }

        public bool AutoPlay { get; set; } = true;
        public long ResumeTime { get; set; } = 0;

        public TimeSpan EmbeddedSubtitlesDelay { get; set; } = TimeSpan.Zero;
        public Dictionary<string,TimeSpan> ExternalSubtitles { get; } = new Dictionary<string, TimeSpan>();
        public int SelectedSubtitlesSpu { get; set; } = -1;
        public string SelectedSubtitlesFileUrl { get; set; } = "";
        
        public bool IsEmbeddedSubtitlesSelected => SelectedSubtitlesSpu != -1;
        public bool IsFileSubtitlesSelected => !string.IsNullOrEmpty(SelectedSubtitlesFileUrl);
    }
}
