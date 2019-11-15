using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Models;

namespace VideoPlayerTrimmer.Services
{
    public interface IVideoLibrary
    {
        Task Refresh();
        Task<List<FolderItem>> GetFolderItemsAsync(bool forceUpdate);
        Task<VideoItem> GetVideoItemAsync(string filePath);
        Task<List<VideoItem>> GetVideoItemsAsync(string folderPath, bool forceUpdate);
        IEnumerable<VideoItem> SearchVideoItems(string title);
        Task MarkAsPlayedAsync(VideoItem video);
        Task SaveVideoItemPreferences(VideoItem videoItem);
        Task<IEnumerable<FavoriteScene>> GetFavoriteScenes(int videoId);
        Task SaveFavoriteScenes(int videoId, IEnumerable<FavoriteScene> scenes);
        void AddVideo(string path);
    }
}
