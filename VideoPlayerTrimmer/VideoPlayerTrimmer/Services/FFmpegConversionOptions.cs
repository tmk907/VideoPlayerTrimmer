namespace VideoPlayerTrimmer.Services
{
    public class FFmpegConversionOptions
    {
        public string VideoCodec { get; set; } = "libx264";
        public string AudioCodec { get; set; } = "aac";
        public string Preset { get; set; } = PresetOptions.Veryfast;
        public string Tune { get; set; } = "";
        public string Container { get; set; } = "mkv";

        public class TuneOptions
        {
            public const string FilmTune = "film";
            public const string AnimationTune = "animation";
            public const string FastTune = "fastdecode";
            public const string ZeroLatencyTune = "zerolatency";
        }

        public class PresetOptions
        {
            public const string Superfast = "superfast";
            public const string Veryfast = "veryfast";
            public const string Medium = "medium";
        }
    }

    public class FFmpegToGifConversionOptions : FFmpegConversionOptions
    {
        public int Width { get; set; } = 480;
    }
}
