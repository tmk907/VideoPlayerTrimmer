using SQLite;
using System;

namespace VideoPlayerTrimmer.Database.Models
{
    public class VideoFileTable
    {
        [PrimaryKey, AutoIncrement]
        public int VideoId { get; set; }
        public long MediaStoreId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Directory { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public TimeSpan Duration { get; set; }
        public int SizeInBytes { get; set; }
        public string Title { get; set; }
        public bool IsNew { get; set; }
        [Indexed]
        public bool IsDeleted { get; set; }
        public TimeSpan Position { get; set; }
    }
}
