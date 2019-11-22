using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Database.Models;

namespace VideoPlayerTrimmer.Services
{
    public class LibraryUpdater
    {
        private readonly VideoDatabase database;
        private readonly IMediaScanner mediaScanner;
        private static SemaphoreSlim semaphore;

        public LibraryUpdater(VideoDatabase database, IMediaScanner mediaScanner)
        {
            this.database = database;
            this.mediaScanner = mediaScanner;
            semaphore = new SemaphoreSlim(1, 1);
        }

        public async Task UpdateAsync()
        {
            App.DebugLog("1");
            await semaphore.WaitAsync();
            App.DebugLog("2");

            var videoSources = await mediaScanner.ScanVideosAsync();
            var oldVideos = (await database.GetAsync<VideoFileTable>(v => v.MediaStoreId)).ToDictionary(e => e.MediaStoreId);
            bool notFirstScan = oldVideos.Count > 0;
            var idsToDelete = oldVideos.ToDictionary(e => e.Key, e => true);
            var newVideos = new List<VideoFileTable>();
            foreach(var source in videoSources)
            {
                if (oldVideos.ContainsKey(source.MediaStoreId))
                {
                    idsToDelete[source.MediaStoreId] = false;
                    if(source.FilePath != oldVideos[source.MediaStoreId].FilePath)
                    {

                    }
                }
                else
                {
                    newVideos.Add(new VideoFileTable()
                    {
                        DateAdded = source.DateAdded,
                        DateModified = source.DateModified,
                        Directory = source.Directory,
                        Duration = source.Duration,
                        FileName = source.FileName,
                        FilePath = source.FilePath,
                        IsDeleted = false,
                        IsNew = true && notFirstScan,
                        MediaStoreId = source.MediaStoreId,
                        Position = TimeSpan.Zero,
                        SizeInBytes = (int)source.SizeInBytes,
                        Title = source.Title,
                        SelectedSubtitles = -1,
                    });
                }
            }
            var videosToDelete = oldVideos.Where(e => idsToDelete[e.Key]).Select(e => e.Value);
            foreach(var video in videosToDelete)
            {
                video.IsDeleted = true;
            }
            await database.InsertAllAsync(newVideos);
            await database.UpdateAllAsync(videosToDelete);

            semaphore.Release();
            App.DebugLog("3");

        }

        public void AddVideo(string path)
        {
            mediaScanner.AddVideo(path);
        }
    }
}
