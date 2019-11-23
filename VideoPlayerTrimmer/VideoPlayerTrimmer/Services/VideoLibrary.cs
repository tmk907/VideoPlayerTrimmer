using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Database.Models;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.Models;

namespace VideoPlayerTrimmer.Services
{
    public class VideoLibrary : IVideoLibrary
    {
        private readonly LibraryUpdater _libraryUpdater;
        private readonly VideoDatabase _database;

        public VideoLibrary(VideoDatabase database, IMediaScanner mediaScanner)
        {
            _database = database;
            _libraryUpdater = new LibraryUpdater(database, mediaScanner);
        }

        private List<FolderItem> _folderItems = new List<FolderItem>();
        private List<VideoItem> _videoItems = new List<VideoItem>();

        public async Task Refresh()
        {
            App.DebugLog("");

            await _libraryUpdater.UpdateAsync();
            var videos = await _database.GetAsync<VideoFileTable>(e => e.VideoId, e => !e.IsDeleted);
            var subtitles = await _database.GetAsync<SubtitleFileTable>(x => x.VideoId);
            var groupedSubtitles = subtitles.ToLookup(x => x.VideoId);
            _videoItems.Clear();
            foreach(var videoRow in videos)
            {
                var video = videoRow.ToVideoItem();
                if (groupedSubtitles.Contains(videoRow.VideoId))
                {
                    video.SubtitleFiles.AddRange(groupedSubtitles[videoRow.VideoId]
                        .Select(x => new SubtitleFile(x.FilePath, videoRow.VideoId)
                        {
                            Delay = x.Delay,
                            IsSelected = x.IsSelected
                        }));
                }
                _videoItems.Add(video);
            }
            RefreshFolders();
        }

        private void RefreshFolders()
        {
            App.DebugLog("");

            _folderItems.Clear();
            foreach(var group in _videoItems.GroupBy(v => v.FolderPath))
            {
                _folderItems.Add(new FolderItem()
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
            App.DebugLog("");

            if (_folderItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            else
            {
                RefreshFolders();
            }
            return _folderItems;
        }

        public Task<VideoItem> GetVideoItemAsync(string filePath)
        {
            App.DebugLog("");

            return Task.FromResult(_videoItems.FirstOrDefault(e => e.FilePath == filePath));
        }

        public async Task<List<VideoItem>> GetVideoItemsAsync(string folderPath, bool forceUpdate = false)
        {
            App.DebugLog("");

            if (_videoItems.Count == 0 || forceUpdate)
            {
                await Refresh();
            }
            if (String.IsNullOrWhiteSpace(folderPath))
            {
                return _videoItems.ToList();
            }
            else
            {
                return _videoItems.Where(e => e.FolderPath == folderPath).ToList();
            }
        }

        public IEnumerable<VideoItem> SearchVideoItems(string title)
        {
            App.DebugLog("");

            var start = _videoItems.Where(v =>
                v.Title.ToLowerInvariant().StartsWith(title.ToLowerInvariant()) ||
                v.FileNameWithoutExtension.ToLowerInvariant().StartsWith(title.ToLowerInvariant()));
            var contains = _videoItems.Where(v =>
                v.Title.ToLowerInvariant().Contains(title.ToLowerInvariant()) ||
                v.FileNameWithoutExtension.ToLowerInvariant().Contains(title.ToLowerInvariant()));
            var result = start.Concat(contains).Distinct();
            return result;
        }

        public async Task MarkAsPlayedAsync(VideoItem video)
        {
            App.DebugLog("");

            video.IsNew = false;
            await _database.UpdateIsNewAsync(video.VideoId, false);
        }

        public async Task UpdateVideoItemPreferences(VideoItem videoItem)
        {
            App.DebugLog("");

            var videoFileTable = await _database.GetFirstAsync<VideoFileTable>(e => e.VideoId == videoItem.VideoId);
            videoFileTable.Position = videoItem.Position;
            videoFileTable.IsNew = false;
            videoFileTable.SelectedSubtitles = videoItem.SelectedSubtitlesId;
            videoFileTable.EmbeddedSubtitlesDelay = videoItem.EmbeddedSubtitlesDelay;

            var subtileFileTable = await _database.GetAsync<SubtitleFileTable>(o => o.FilePath, c => c.VideoId == videoItem.VideoId);
            var updatedFileSubtitles = new List<SubtitleFileTable>();
            var newFileSubtitles = new List<SubtitleFileTable>();
            foreach(var sub in videoItem.SubtitleFiles)
            {
                var subTable = subtileFileTable.Find(x => x.FilePath == sub.FilePath);
                if (subTable == null)
                {
                    newFileSubtitles.Add(new SubtitleFileTable()
                    {
                        Delay = sub.Delay,
                        FilePath = sub.FilePath,
                        VideoId = videoItem.VideoId,
                        IsSelected = sub.IsSelected
                    });
                }
                else
                {
                    if(subTable.Delay != sub.Delay || subTable.IsSelected != sub.IsSelected)
                    {
                        subTable.Delay = sub.Delay;
                        subTable.IsSelected = sub.IsSelected;
                        updatedFileSubtitles.Add(subTable);
                    }
                    subtileFileTable.Remove(subTable);
                }
            }

            await _database.UpdateAsync(videoFileTable);
            await _database.UpdateAllAsync(subtileFileTable);
            await _database.InsertAllAsync(newFileSubtitles);
            foreach (var sub in subtileFileTable)
            {
                await _database.DeleteAsync(sub);
            }
        }

        public async Task<IEnumerable<FavoriteScene>> GetFavoriteScenes(int videoId)
        {
            App.DebugLog("");

            var list = await _database.GetAsync<FavoriteSceneTable>(s => s.Position, s => s.VideoId == videoId);
            return list.Select(s => new FavoriteScene() { Position = s.Position, ThumbnailPath = s.ThumbnailPath });
        }

        public async Task SaveFavoriteScenes(int videoId, IEnumerable<FavoriteScene> scenes)
        {
            App.DebugLog("");

            await _database.DeleteAsync<FavoriteSceneTable>(s => s.VideoId == videoId);
            var list = scenes.Select(s => new FavoriteSceneTable()
            {
                Position = s.Position,
                ThumbnailPath = s.ThumbnailPath,
                VideoId = videoId
            });
            await _database.InsertAllAsync(list);
        }

        public void AddVideo(string path)
        {
            App.DebugLog("");

            _libraryUpdater.AddVideo(path);
        }
    }
}
