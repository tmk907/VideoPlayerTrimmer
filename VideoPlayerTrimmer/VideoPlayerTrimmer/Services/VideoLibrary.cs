using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Database.Models;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.Models;

namespace VideoPlayerTrimmer.Services
{
    public class VideoLibrary : IVideoLibrary
    {
        private readonly LibraryUpdater libraryUpdater;
        private readonly VideoDatabase database;

        public VideoLibrary(VideoDatabase database, IMediaScanner mediaScanner)
        {
            this.database = database;
            libraryUpdater = new LibraryUpdater(database, mediaScanner);
        }

        private List<FolderItem> folderItems = new List<FolderItem>();
        private List<VideoItem> videoItems = new List<VideoItem>();

        public async Task Refresh()
        {
            await libraryUpdater.UpdateAsync();
            var videos = await database.Get<VideoFileTable>(e => e.VideoId, e => !e.IsDeleted);
            folderItems.Clear();
            videoItems.Clear();
            foreach(var directoryGroup in videos.GroupBy(e => e.Directory))
            {
                folderItems.Add(new FolderItem()
                {
                    AreNewVideos = directoryGroup.Any(e => e.IsNew),
                    FolderName = Path.GetFileName(directoryGroup.Key),
                    FolderPath = directoryGroup.Key,
                    VideoCount = directoryGroup.Count()
                });
            }
            foreach(var video in videos)
            {
                videoItems.Add(video.ToVideoItem());
            }
        }

        public async Task<List<FolderItem>> GetFolderItemsAsync(bool forceUpdate = false)
        {
            if (folderItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            return folderItems;
        }

        public Task<VideoItem> GetVideoItemAsync(string filePath)
        {
            return Task.FromResult(videoItems.FirstOrDefault(e => e.FilePath == filePath));
        }

        public async Task<List<VideoItem>> GetVideoItemsAsync(string folderPath, bool forceUpdate = false)
        {
            if (folderItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            return videoItems.Where(e => e.FolderPath == folderPath).ToList();
        }

        public void MarkAsPlayed(int videoId)
        {
            throw new NotImplementedException();
        }

        public async Task SaveVideoItemPreferences(VideoItem videoItem)
        {
            var videoFileTable = await database.GetFirst<VideoFileTable>(e => e.VideoId == videoItem.VideoId);
            videoFileTable.Position = videoItem.Preferences.Position;
            await database.Update(videoFileTable);
        }
    }
}
