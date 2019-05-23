using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Services
{
    public interface IFFmpegService
    {
        Task Test(string path);
        Task Trim(string[] cmd);
    }
}
