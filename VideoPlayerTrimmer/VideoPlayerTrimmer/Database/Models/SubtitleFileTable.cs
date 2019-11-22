using SQLite;
using System;

namespace VideoPlayerTrimmer.Database.Models
{
    public class SubtitleFileTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Delay { get; set; }
        public int VideoId { get; set; }
    }
}
