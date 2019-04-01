using System;
using System.IO;

namespace VideoPlayerTrimmer.Models
{
    public class VideoSource
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string Directory { get { return Path.GetDirectoryName(FilePath); } }
        public string FolderName { get { return Path.GetFileName(Directory); } }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public TimeSpan Duration { get; set; }
        public long Id { get; set; }
        public long SizeInBytes { get; set; }
        public string Title { get; set; }
    }
}
