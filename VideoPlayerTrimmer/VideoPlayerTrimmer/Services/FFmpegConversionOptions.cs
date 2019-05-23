using System;
using System.Collections.Generic;
using System.Text;

namespace VideoPlayerTrimmer.Services
{
    public class FFmpegConversionOptions
    {
        public FFmpegConversionOptions(TimeSpan startPosition, TimeSpan endPosition, string inputFilePath, string outputFilePath)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
        }

        public TimeSpan StartPosition { get; set; }
        public TimeSpan EndPosition { get; set; }
        public TimeSpan Duration { get => EndPosition - StartPosition; }

        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }

    public class FFmpegToVideoConversionOptions : FFmpegConversionOptions
    {
        public FFmpegToVideoConversionOptions(TimeSpan startPosition, TimeSpan endPosition, string inputFilePath, string outputFilePath) : base(startPosition, endPosition, inputFilePath, outputFilePath)
        {
        }

        public string VideoCodec { get; set; } = "libx264";
        public string AudioCodec { get; set; } = "aac";
        public string Preset { get; set; } = "superfast";
    }

    public class FFmpegToGifConversionOptions : FFmpegConversionOptions
    {
        public FFmpegToGifConversionOptions(TimeSpan startPosition, TimeSpan endPosition, string inputFilePath, string outputFilePath) : base(startPosition, endPosition, inputFilePath, outputFilePath)
        {
        }

        public int Width { get; set; } = 480;
    }
}
