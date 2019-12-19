using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VideoPlayerTrimmer.Services
{
    public class ConversionOptions
    {
        public string FilePath { get; set; }
        public string OutputFolderPath { get; set; }
        public string OutputFileName { get; set; }
        public string OutputFilePath => Path.Combine(OutputFolderPath, $"{OutputFileName}.{FFmpegConversionOptions.Container}");

        public TimeSpan StartPosition { get; set; }
        public TimeSpan EndPosition { get; set; }
        public TimeSpan TotalDuration => EndPosition - StartPosition;

        public int EmbeddedSubtitlesIndex { get; set; } = -1;
        public bool AddEmbeddedSubtitles => EmbeddedSubtitlesIndex != -1;
        public string FileSubtitlesPath { get; set; }
        public bool AddFileSubtitles => !string.IsNullOrEmpty(FileSubtitlesPath);
        public string FileSubtitlesOutputPath => Path.Combine(OutputFolderPath, $"{OutputFileName}{Path.GetExtension(FileSubtitlesPath)}");
        public bool HardSub { get; set; }

        public bool FastMode { get; set; }

        public FFmpegConversionOptions FFmpegConversionOptions { get; set; }
    }

    public class ConverterHelper
    {
        private readonly IVideoLibrary _videoLibrary;
        private readonly IFFmpegConverter _fFmpegConverter;
        private readonly MediaPlayerBuilder _playerService;

        private string outputPath = "";

        public ConverterHelper(IVideoLibrary videoLibrary,
            IFFmpegConverter fFmpegConverter, MediaPlayerBuilder playerService)
        {
            _videoLibrary = videoLibrary;
            _fFmpegConverter = fFmpegConverter;
            _playerService = playerService;

            _fFmpegConverter.ConversionProgressChanged += FFmpegConverter_ProgressChanged;
            _fFmpegConverter.ConversionStarted += FFmpegConverter_ConversionStarted;
            _fFmpegConverter.ConversionEnded += FFmpegConverter_ConversionEnded;
        }

        public event Action ConversionStarted;
        public event Action<int> ConversionProgress;
        public event Action<string> ConversionEnded;
        private event Action<ConversionOptions> SubtitlesTrimmed;
        private ConversionOptions _options;
        private bool _canDeleteOutputFileSubtitles = false;

        public Task ConvertVideoAsync(ConversionOptions options)
        {
            outputPath = options.OutputFilePath;
            if (options.FastMode)
            {
                return SaveUsingVLCAsync(options);
            }
            else
            {
                if (options.AddFileSubtitles)
                {
                    _options = options;
                    var subCommands = TrimSubtitlesCommand(options);
                    SubtitlesTrimmed += ConvertVideoWithSubtitles;
                    return _fFmpegConverter.Convert(subCommands, options.StartPosition, options.EndPosition);
                }
                else
                {
                    var commands = CovertVideoCommand(options);
                    return _fFmpegConverter.Convert(commands, options.StartPosition, options.EndPosition);
                }
            }
        }

        private async void ConvertVideoWithSubtitles(ConversionOptions options)
        {
            SubtitlesTrimmed -= ConvertVideoWithSubtitles;
            ConversionStarted?.Invoke();
            var commands = CovertVideoCommand(options);
            await Task.Delay(500);
            _canDeleteOutputFileSubtitles = true;
            await _fFmpegConverter.Convert(commands, options.StartPosition, options.EndPosition);
        }

        private async Task SaveUsingVLCAsync(ConversionOptions options)
        {
            ConversionStarted?.Invoke();

            int start = (int)options.StartPosition.TotalSeconds;
            int end = (int)options.EndPosition.TotalSeconds;
            //string startFull = start + "." + options.StartPosition.Milliseconds.ToString("D3");
            //string endFull = end + "." + options.EndPosition.Milliseconds.ToString("D3");
            var mp = _playerService.GetMediaPlayerForTrimming(options.FilePath, options.OutputFilePath, start, end);
            mp.Play();
            mp.EndReached += Mp_EndReached;
            while (true)
            {
                await Task.Delay(1000);
                if (!mp.IsPlaying)
                {
                    break;
                }
            }
            mp.Stop();
            App.DebugLog("stopped");
            mp.EndReached -= Mp_EndReached;
            mp.Dispose();
            AddToLibrary(outputPath);
            ConversionEnded?.Invoke(outputPath);
        }

        private void AddToLibrary(string outputPath)
        {
            if (System.IO.File.Exists(outputPath))
            {
                _videoLibrary.AddVideo(outputPath);
            }
        }

        private void Mp_EndReached(object sender, EventArgs e)
        {
            App.DebugLog("Video saved");
        }

        private void FFmpegConverter_ConversionEnded(object sender, EventArgs e)
        {
            App.DebugLog("FFmpegConverter_ConversionEnded IsConverting");
            AddToLibrary(outputPath);
            ConversionEnded?.Invoke(outputPath);
            SubtitlesTrimmed?.Invoke(_options);
            if (_canDeleteOutputFileSubtitles == true)
            {
                _canDeleteOutputFileSubtitles = false;
                DeleteOutputFileSubtitles();
            }
        }

        private void FFmpegConverter_ConversionStarted(object sender, EventArgs e)
        {
            App.DebugLog("FFmpegConverter_ConversionStarted IsConverting");
            ConversionStarted?.Invoke();
        }

        private void FFmpegConverter_ProgressChanged(object sender, int e)
        {
            App.DebugLog("FFmpegConverter_ProgressChanged IsConverting");
            ConversionProgress?.Invoke(e);
        }

        private void DeleteOutputFileSubtitles()
        {
            if (File.Exists(_options.FileSubtitlesOutputPath))
            {
                File.Delete(_options.FileSubtitlesOutputPath);
            }
        }

        public List<string> TrimSubtitlesCommand(ConversionOptions options)
        {
            var commands = new List<string>();
            //if (options.)
            //{
            //    commands.Add("-sub_charenc");
            //    commands.Add(options.);
            //}
            commands.Add("-ss");
            commands.Add(options.StartPosition.ToString("g").Replace(',', '.'));
            commands.Add("-i");
            commands.Add(options.FileSubtitlesPath);
            commands.Add("-to");
            commands.Add(options.EndPosition.ToString("g").Replace(',', '.'));
            commands.Add(options.FileSubtitlesOutputPath);
            return commands;
        }

        public List<string> CovertVideoCommand(ConversionOptions options)
        {
            var commands = new List<string>();
            commands.AddOption("-y");
            commands.AddOption("-ss", options.StartPosition.ToString("g").Replace(',', '.'));
            
            commands.AddInput(options.FilePath);
            if (options.AddFileSubtitles && !options.HardSub)
            {
                commands.AddInput(options.FileSubtitlesPath);
            }

            commands.AddOption("-map", "0:v");
            commands.AddOption("-map", "0:a");

            if (options.AddFileSubtitles)
            {
                if (options.HardSub)
                {
                    commands.Add("-vf");
                    commands.AddOption($"subtitles='{options.FileSubtitlesOutputPath}'");
                }
                else
                {
                    commands.AddOption("-map", "1:s");
                    commands.AddOption("-map", "0:s?");
                }
            }
            else if (options.AddEmbeddedSubtitles)
            {
                if (options.HardSub)
                {
                    commands.AddOption("-map", $"0:s:{options.EmbeddedSubtitlesIndex}");
                    
                    // this subs arent trimmed
                    //commands.Add("-vf");
                    //commands.AddOption($"subtitles='{options.FilePath}:si={options.EmbeddedSubtitlesIndex}'");
                }
                else
                {
                    commands.AddOption("-map", $"0:s:{options.EmbeddedSubtitlesIndex}");
                }
            }
            else
            {
                commands.AddOption("-map", "0:s?");
            }

            commands.AddOption("-t", (options.EndPosition - options.StartPosition).ToString("g").Replace(',', '.'));

            commands.AddOption("-preset", options.FFmpegConversionOptions.Preset);
            if (options.FFmpegConversionOptions.Tune != "")
            {
                commands.AddOption("-tune", options.FFmpegConversionOptions.Tune);
            }
            commands.AddOption("-c:v", options.FFmpegConversionOptions.VideoCodec);
            commands.AddOption("-c:a", options.FFmpegConversionOptions.AudioCodec);
            commands.AddOption("-avoid_negative_ts", "1");
            
            commands.AddOutput(options.OutputFilePath);

            return commands;
        }
    }
}
