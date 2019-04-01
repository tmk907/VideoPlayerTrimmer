using SQLite;
using System;

namespace VideoPlayerTrimmer.Database.Models
{
    public class VideoFileTable
    {
        [PrimaryKey]
        public int MediaStoreId { get; set; }
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
    }
}
