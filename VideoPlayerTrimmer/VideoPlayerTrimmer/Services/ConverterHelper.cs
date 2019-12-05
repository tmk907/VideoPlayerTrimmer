using System;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.FilePicker;

namespace VideoPlayerTrimmer.Services
{
    public class ConversionOptions
    {
        public string FilePath { get; set; }
        public string FileNameWithoutExtension { get; set; }
        public TimeSpan StartPosition { get; set; }
        public TimeSpan EndPosition { get; set; }
        public TimeSpan TotalDuration => EndPosition - StartPosition;

        public bool FastMode { get; set; }
        public bool SaveAsGif { get; set; }
    }

    public class ConverterHelper
    {
        private readonly IFileService _fileService;
        private readonly IVideoLibrary _videoLibrary;
        private readonly IFFmpegConverter _fFmpegConverter;
        private readonly MediaPlayerBuilder _playerService;

        public ConverterHelper(IFileService fileService, IVideoLibrary videoLibrary, 
            IFFmpegConverter fFmpegConverter, MediaPlayerBuilder playerService)
        {
            _fileService = fileService;
            _videoLibrary = videoLibrary;
            _fFmpegConverter = fFmpegConverter;
            _playerService = playerService;

            _fFmpegConverter.ConversionProgressChanged += FFmpegConverter_ProgressChanged;
            _fFmpegConverter.ConversionStarted += FFmpegConverter_ConversionStarted;
            _fFmpegConverter.ConversionEnded += FFmpegConverter_ConversionEnded;

        }

        public event Action ConversionStarted;
        public event Action ConversionEnded;

        private string BuildOutputPath(string fileNameWithoutExtension, TimeSpan startPosition)
        {
            string newFileName = $@"{fileNameWithoutExtension} [{startPosition.ToVideoDuration()}].mp4";
            var internalMemoryPath = _fileService.GetInternalMemoryRootPath();
            string folderName = "VideoPlayerTrimmer";
            string folderPath = System.IO.Path.Combine(internalMemoryPath, folderName);
            try
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            catch (Exception ex)
            {
                App.DebugLog(ex.ToString());
            }
            string outputPath = System.IO.Path.Combine(folderPath, newFileName);
            return outputPath;
        }

        public async Task ConvertVideoAsync(ConversionOptions options)
        {
            if (options.FastMode)
            {
                await SaveUsingVLCAsync(options);
            }
            else
            {
                if (options.SaveAsGif && options.TotalDuration.TotalSeconds < 20)
                {
                    await SaveAsGif(options);
                }
                else
                {
                    await ConvertVideo(options);
                }
            }
        }

        private async Task SaveUsingVLCAsync(ConversionOptions options)
        {
            ConversionStarted?.Invoke();

            int start = (int)options.StartPosition.TotalSeconds;
            int end = (int)options.EndPosition.TotalSeconds;
            string startFull = start + "." + options.StartPosition.Milliseconds.ToString("D3");
            string endFull = end + "." + options.EndPosition.Milliseconds.ToString("D3");
            var outputPath = BuildOutputPath(options.FileNameWithoutExtension, options.StartPosition);
            var mp = _playerService.GetMediaPlayerForTrimming(options.FilePath, outputPath, start, end);
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
            ConversionEnded?.Invoke();
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

        private string outputPath = "";

        private async Task ConvertVideo(ConversionOptions options)
        {
            int start = (int)options.StartPosition.TotalSeconds;
            outputPath = BuildOutputPath(options.FileNameWithoutExtension, options.StartPosition);
            var ffmpegOptions = 
                new FFmpegToVideoConversionOptions(options.StartPosition, options.EndPosition, options.FilePath, outputPath);
            await _fFmpegConverter.CovertToVideo(ffmpegOptions);
        }

        private async Task SaveAsGif(ConversionOptions options)
        {
            string outputFilename = $@"{options.FileNameWithoutExtension} [{options.StartPosition.ToVideoDuration()}].gif";
            var outputPath = BuildOutputPath(options.FileNameWithoutExtension, options.StartPosition);
            var ffmpegOptions = 
                new FFmpegToGifConversionOptions(options.StartPosition, options.EndPosition, options.FilePath, outputPath);
            await _fFmpegConverter.ConvertToGif(ffmpegOptions);
        }

        private void FFmpegConverter_ConversionEnded(object sender, EventArgs e)
        {
            App.DebugLog("FFmpegConverter_ConversionEnded IsConverting");
            AddToLibrary(outputPath);
            ConversionEnded?.Invoke();
        }

        private void FFmpegConverter_ConversionStarted(object sender, EventArgs e)
        {
            App.DebugLog("FFmpegConverter_ConversionStarted IsConverting");
            ConversionStarted?.Invoke();
        }

        private void FFmpegConverter_ProgressChanged(object sender, int e)
        {
            App.DebugLog("FFmpegConverter_ProgressChanged IsConverting");
            ConversionStarted?.Invoke();
        }
    }
}
