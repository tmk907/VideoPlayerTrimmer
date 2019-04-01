using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Framework;

namespace VideoPlayerTrimmer.Models
{
    public class FolderItem : BindableBase
    {
        public string FolderName { get; set; }

        public string FolderPath { get; set; }

        private bool areNewVideos;
        public bool AreNewVideos
        {
            get { return areNewVideos; }
            set { SetProperty(ref areNewVideos, value); }
        }

        private int videoCount;
        public int VideoCount
        {
            get { return videoCount; }
            set { SetProperty(ref videoCount, value); }
        }
    }
}
