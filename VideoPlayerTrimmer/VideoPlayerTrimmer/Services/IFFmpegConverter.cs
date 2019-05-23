using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Services
{
    public interface IFFmpegConverter
    {
        event EventHandler ConversionStarted;
        event EventHandler ConversionEnded;
        event EventHandler<int> ConversionProgressChanged;
        Task CovertToVideo(FFmpegToVideoConversionOptions options);
        Task ConvertToGif(FFmpegToGifConversionOptions options);
    }
}
