using System;
using System.IO;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class SubtitleFile : BindableBase
    {
        public SubtitleFile(string filePath, int videoId)
        {
            VideoId = videoId;
            FilePath = filePath;
            IsSelected = false;
            Delay = TimeSpan.Zero;
        }

        public int VideoId { get; }
        public string FilePath { get; }
        public string FileName => Path.GetDirectoryName(FilePath);
        public string FileUrl => "file://" + FilePath;
        public TimeSpan Delay { get; set; }
        public string Encoding { get; set; }

        protected bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetProperty(ref isSelected, value); }
        }
    }
}
