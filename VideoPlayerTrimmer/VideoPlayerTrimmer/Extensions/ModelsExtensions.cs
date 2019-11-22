using VideoPlayerTrimmer.Database.Models;
using VideoPlayerTrimmer.Models;

namespace VideoPlayerTrimmer.Extensions
{
    public static class VideoFileTableExtensions
    {
        public static VideoItem ToVideoItem(this VideoFileTable table)
        {
            return new VideoItem()
            {
                Duration = table.Duration,
                FileName = table.FileName,
                FilePath = table.FilePath,
                FolderPath = table.Directory,
                IsNew = table.IsNew,
                MediaStoreId = table.MediaStoreId,
                Title = table.Title,
                VideoId = table.VideoId,
                Position = table.Position,
                SelectedSubtitlesId = table.SelectedSubtitles,
                EmbeddedSubtitlesDelay = table.EmbeddedSubtitlesDelay,
            };
        }
    }
}
