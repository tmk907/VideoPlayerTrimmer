using SQLite;
using System;

namespace VideoPlayerTrimmer.Database.Models
{
    public class FavoriteSceneTable
    {
        [PrimaryKey, AutoIncrement]
        public int FavSceneId { get; set; }
        
        [NotNull]
        public int VideoId { get; set; }
        [NotNull]
        public TimeSpan Position { get; set; }
        public string ThumbnailPath { get; set; }
    }
}
