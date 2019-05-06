using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Models
{
    public class FavoriteScene
    {
        public TimeSpan Position { get; set; }
        public string ThumbnailPath { get; set; }
        public string SnapshotPath { get; set; }
    }
}
