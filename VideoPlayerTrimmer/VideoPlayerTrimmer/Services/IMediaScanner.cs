using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Models;

namespace VideoPlayerTrimmer.Services
{
    public interface IMediaScanner
    {
        Task<List<VideoSource>> ScanVideosAsync();
        void AddVideo(string path);
    }
}
