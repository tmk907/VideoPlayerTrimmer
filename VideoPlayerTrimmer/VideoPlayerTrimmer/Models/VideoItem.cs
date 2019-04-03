using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class VideoItem : BindableBase
    {
        public int VideoId { get; set; }

        public string FilePath { get; set; }

        public string FileName { get; set; }

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

        public VideoItemPreferences Preferences { get; set; }
    }
}
