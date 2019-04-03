using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Database.Models;

namespace VideoPlayerTrimmer.Services
{
    public class LibraryUpdater
    {
        private readonly VideoDatabase database;
        private readonly IMediaScanner mediaScanner;

        public LibraryUpdater(VideoDatabase database, IMediaScanner mediaScanner)
        {
            this.database = database;
            this.mediaScanner = mediaScanner;
        }

        public async Task UpdateAsync()
        {
            var videoSources = await mediaScanner.ScanVideosAsync();
            var oldVideos = (await database.Get<VideoFileTable>(v => v.MediaStoreId)).ToDictionary(e => e.MediaStoreId);
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
                        IsNew = true,
                        MediaStoreId = source.MediaStoreId,
                        Position = TimeSpan.Zero,
                        SizeInBytes = (int)source.SizeInBytes,
                        Title = source.Title,
                    });
                }
            }
            var videosToDelete = oldVideos.Where(e => idsToDelete[e.Key]).Select(e => e.Value);
            foreach(var video in videosToDelete)
            {
                video.IsDeleted = true;
            }
            await database.InsertAll(newVideos);
            await database.UpdateAll(videosToDelete);
        }
    }
}
