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
        void MarkAsPlayed(int videoId);
        Task SaveVideoItemPreferences(VideoItem videoItem);
    }
}
