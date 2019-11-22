using System;
using System.Collections.Generic;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class StartupConfiguration
    {
        public bool AutoPlay { get; set; } = true;
        public long ResumeTime { get; set; } = 0;

        public TimeSpan SelectedSubtitlesDelay { get; set; } = TimeSpan.Zero;
        public List<string> ExternalSubtitles { get; } = new List<string>();
        public int SelectedSubtitlesSpu { get; set; } = -1;
        public string SelectedSubtitlesFileUrl { get; set; } = "";
        
        public bool IsEmbeddedSubtitlesSelected => SelectedSubtitlesSpu != -1;
        public bool IsFileSubtitlesSelected => !string.IsNullOrEmpty(SelectedSubtitlesFileUrl);
    }
}
