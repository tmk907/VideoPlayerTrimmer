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
            var videos = await database.GetAsync<VideoFileTable>(e => e.VideoId, e => !e.IsDeleted);
            videoItems.Clear();
            foreach(var video in videos)
            {
                videoItems.Add(video.ToVideoItem());
            }
            RefreshFolders();
        }

        private void RefreshFolders()
        {
            folderItems.Clear();
            foreach(var group in videoItems.GroupBy(v => v.FolderPath))
            {
                folderItems.Add(new FolderItem()
                {
                    AreNewVideos = group.Any(e => e.IsNew),
                    FolderName = Path.GetFileName(group.Key),
                    FolderPath = group.Key,
                    VideoCount = group.Count()
                });
            }
        }

        public async Task<List<FolderItem>> GetFolderItemsAsync(bool forceUpdate = false)
        {
            if (folderItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            else
            {
                RefreshFolders();
            }
            return folderItems;
        }

        public Task<VideoItem> GetVideoItemAsync(string filePath)
        {
            return Task.FromResult(videoItems.FirstOrDefault(e => e.FilePath == filePath));
        }

        public async Task<List<VideoItem>> GetVideoItemsAsync(string folderPath, bool forceUpdate = false)
        {
            if (videoItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            return videoItems.Where(e => e.FolderPath == folderPath).ToList();
        }

        public async Task MarkAsPlayedAsync(VideoItem video)
        {
            video.IsNew = false;
            await database.UpdateIsNewAsync(video.VideoId, false);
        }

        public async Task SaveVideoItemPreferences(VideoItem videoItem)
        {
            var videoFileTable = await database.GetFirstAsync<VideoFileTable>(e => e.VideoId == videoItem.VideoId);
            videoFileTable.Position = videoItem.Preferences.Position;
            await database.UpdateAsync(videoFileTable);
        }

        public async Task<IEnumerable<FavoriteScene>> GetFavoriteScenes(int videoId)
        {
            var list = await database.GetAsync<FavoriteSceneTable>(s => s.Position, s => s.VideoId == videoId);
            return list.Select(s => new FavoriteScene() { Position = s.Position, ThumbnailPath = s.ThumbnailPath });
        }

        public async Task SaveFavoriteScenes(int videoId, IEnumerable<FavoriteScene> scenes)
        {
            await database.DeleteAsync<FavoriteSceneTable>(s => s.VideoId == videoId);
            var list = scenes.Select(s => new FavoriteSceneTable()
            {
                Position = s.Position,
                ThumbnailPath = s.ThumbnailPath,
                VideoId = videoId
            });
            await database.InsertAllAsync(list);
        }
    }
}
