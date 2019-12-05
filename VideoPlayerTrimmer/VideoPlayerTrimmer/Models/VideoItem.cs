using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class VideoItem : BindableBase
    {
        public int VideoId { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

        public string FileNameWithoutExtension { get { return Path.GetFileNameWithoutExtension(FilePath); } }

        public string Directory { get { return Path.GetDirectoryName(FilePath); } }

        public string FolderPath { get; set; }

        public long MediaStoreId { get; set; }

        private bool isNew;
        public bool IsNew
        {
            get { return isNew; }
            set { SetProperty(ref isNew, value); }
        }

        public string Title { get; set; }

        public TimeSpan Duration { get; set; }

        public ObservableCollection<SubtitleFile> SubtitleFiles { get; set; } = new ObservableCollection<SubtitleFile>();

        public TimeSpan Position { get; set; }

        public SubtitleFile SelectedSubtitleFile { get { return SubtitleFiles.FirstOrDefault(x => x.IsSelected); } }
        public bool IsFileSubtitleSelected { get { return SelectedSubtitleFile != null; } }

        public int SelectedSubtitlesId { get; set; }

        public TimeSpan EmbeddedSubtitlesDelay { get; set; }
    }
}
