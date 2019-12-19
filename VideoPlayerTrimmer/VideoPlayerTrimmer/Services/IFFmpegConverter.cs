using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Services
{
    public interface IFFmpegConverter
    {
        event EventHandler ConversionStarted;
        event EventHandler ConversionEnded;
        event EventHandler<int> ConversionProgressChanged;
        Task Convert(List<string> commands, TimeSpan start, TimeSpan end);
    }
}
