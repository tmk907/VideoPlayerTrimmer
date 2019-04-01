using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class VideoItem : BindableBase
    {
        public string FilePath { get; set; }

        public string FileName { get; set; }

        public string FolderPath { get; set; }

        private bool isNew;
        public bool IsNew
        {
            get { return isNew; }
            set { SetProperty(ref isNew, value); }
        }

        public TimeSpan Duration { get; set; }
    }
}
