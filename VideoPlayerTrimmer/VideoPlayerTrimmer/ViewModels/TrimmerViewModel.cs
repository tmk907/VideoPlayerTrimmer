using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class TrimmerViewModel : BaseViewModel, INavigatedAware
    {
        public TrimmerViewModel(IVideoLibrary videoLibrary, INavigationService navigationService, 
            MediaPlayerService playerService, IFFmpegConverter fFmpegConverter)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            this.navigationService = navigationService;
            this.playerService = playerService;
            this.fFmpegConverter = fFmpegConverter;
            fFmpegConverter.ConversionProgressChanged += FFmpegConverter_ProgressChanged;
            fFmpegConverter.ConversionStarted += FFmpegConverter_ConversionStarted;
            fFmpegConverter.ConversionEnded += FFmpegConverter_ConversionEnded;

            MediaHelper = new MediaPlayerHelper(playerService);
            MediaHelper.IsPausedByUser = true;
            MediaHelper.MediaPlayerReady += MediaPlayerHelper_MediaPlayerReady;
            MediaHelper.PlaybackStateChanged += MediaHelper_PlaybackStateChanged;

            GoToNextFavSceneCommand = new DelegateCommand(GoToNextFavScene);
            GoToPrevFavSceneCommand = new DelegateCommand(GoToPrevFavScene);
            IncrementPositionCommand = new DelegateCommand<object>((e) => IncrementPosition(e));
            DecrementPositionCommand = new DelegateCommand<object>((e) => DecrementPosition(e));
            JumpToStartCommand = new DelegateCommand(() => JumpToStart());
            TogglePlayPauseCommand = new DelegateCommand(TogglePlayPause);

            OffsetOptions = new ObservableCollection<OffsetOption>()
            {
                new OffsetOption("1 sec", TimeSpan.FromSeconds(1)),
                new OffsetOption("10 sec", TimeSpan.FromSeconds(10)),
                new OffsetOption("1 min", TimeSpan.FromMinutes(1)),
                new OffsetOption("10 min", TimeSpan.FromMinutes(10)),
            };
            SelectedOffsetOption = OffsetOptions.First();

            EndPosition = TimeSpan.FromMinutes(1);
            TotalDuration = EndPosition;
        }

        private void FFmpegConverter_ConversionEnded(object sender, EventArgs e)
        {
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(()=>
            {
                IsConverting = false;
                if (!String.IsNullOrEmpty(convertedVideoPath))
                {
                    videoLibrary.AddVideo(convertedVideoPath);
                }
                convertedVideoPath = null;
                App.DebugLog("FFmpegConverter_ConversionEnded IsConverting");
            });
        }

        private void FFmpegConverter_ConversionStarted(object sender, EventArgs e)
        {
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                IsConverting = true;
                App.DebugLog("FFmpegConverter_ConversionStarted IsConverting");
            });
        }

        private void FFmpegConverter_ProgressChanged(object sender, int e)
        {
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                IsConverting = true;
                App.DebugLog("FFmpegConverter_ProgressChanged IsConverting");
            });
        }

        private void MediaHelper_TimeChanged(object sender, LibVLCSharp.Shared.MediaPlayerTimeChangedEventArgs e)
        {
            var time = TimeSpan.FromMilliseconds(e.Time);
            CurrentPosition = time;
        }

        private void MediaPlayerHelper_MediaPlayerReady(object sender, EventArgs e)
        {
            var t = TimeSpan.FromMilliseconds(MediaHelper.MediaPlayer.Length);
            if (MediaHelper.MediaPlayer.Fps > 0)
            {
                var frameDuration = 1.0 / MediaHelper.MediaPlayer.Fps;
                var f = OffsetOptions.FirstOrDefault(o => o.Name == "1 Frame");
                if (f != null)
                {
                    OffsetOptions.Remove(f);
                }
                OffsetOptions.Add(new OffsetOption("1 Frame", TimeSpan.FromSeconds(frameDuration)));
            }
            if (t.TotalMilliseconds!= totalDuration.TotalMilliseconds)
            {

            }
        }

        private void MediaHelper_PlaybackStateChanged(object sender, PlaybackStateEventArgs e)
        {
            IsPlaying = e.PlaybackState == PlaybackState.Playing;
        }

        public MediaPlayerHelper MediaHelper { get; }

        private TimeSpan startPosition = TimeSpan.Zero;
        public TimeSpan StartPosition
        {
            get { return startPosition; }
            set
            {
                SetProperty(ref startPosition, value);
                MediaHelper.SeekTo(startPosition);
            }
        }

        private TimeSpan endPosition;
        public TimeSpan EndPosition
        {
            get { return endPosition; }
            set
            {
                SetProperty(ref endPosition, value);
                MediaHelper.SeekTo(endPosition);
            }
        }

        public TimeSpan SelectedDuration { get => endPosition - startPosition; }

        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set { SetProperty(ref currentPosition, value); }
        }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        private TimeSpan defaultFavSceneDuration = TimeSpan.FromSeconds(10);

        private int selectedfavSceneIndex = -1;
        private List<FavoriteScene> favoriteScenes = new List<FavoriteScene>();

        private bool isfavoriteScene = false;
        public bool IsFavoriteScene
        {
            get { return isfavoriteScene; }
            set { SetProperty(ref isfavoriteScene, value); }
        }

        private bool noFileSelected = true;
        public bool NoFileSelected
        {
            get { return noFileSelected; }
            set { SetProperty(ref noFileSelected, value); }
        }

        private OffsetOption selectedOffsetOption;
        public OffsetOption SelectedOffsetOption
        {
            get { return selectedOffsetOption; }
            set { SetProperty(ref selectedOffsetOption, value); }
        }

        public ObservableCollection<OffsetOption> OffsetOptions { get; }

        private bool saveToGif = false;
        public bool SaveToGif
        {
            get { return saveToGif; }
            set { SetProperty(ref saveToGif, value); }
        }

        private bool fastTrim = true;
        public bool FastTrim
        {
            get { return fastTrim; }
            set { SetProperty(ref fastTrim, value); }
        }

        private bool isConverting = false;
        public bool IsConverting
        {
            get { return isConverting; }
            set { SetProperty(ref isConverting, value); }
        }

        public DelegateCommand GoToPrevFavSceneCommand { get; }
        public DelegateCommand GoToNextFavSceneCommand { get; }
        public DelegateCommand TogglePlayPauseCommand { get; }
        public DelegateCommand<object> IncrementPositionCommand { get; }
        public DelegateCommand<object> DecrementPositionCommand { get; }
        public DelegateCommand JumpToStartCommand { get; }

        private TimeSpan previewPosition = TimeSpan.Zero;

        private TimeSpan totalDuration;
        public TimeSpan TotalDuration
        {
            get { return totalDuration; }
            set { SetProperty(ref totalDuration, value); }
        }

        private void GoToPrevFavScene()
        {
            if (selectedfavSceneIndex == -1) return;
            selectedfavSceneIndex--;
            if(selectedfavSceneIndex == -1)
            {
                IsFavoriteScene = false;
                StartPosition = TimeSpan.Zero;
                EndPosition = totalDuration;
            }
            else
            {
                IsFavoriteScene = true;
                StartPosition = favoriteScenes[selectedfavSceneIndex].Position;
                EndPosition = Min(totalDuration, StartPosition + defaultFavSceneDuration);
            }
            previewPosition = StartPosition;
        }

        private void GoToNextFavScene()
        {
            if (selectedfavSceneIndex + 1 == favoriteScenes.Count) return;
            selectedfavSceneIndex++;
            IsFavoriteScene = true;
            StartPosition = favoriteScenes[selectedfavSceneIndex].Position;
            EndPosition = Min(totalDuration, startPosition + defaultFavSceneDuration);
        }

        private void TogglePlayPause()
        {
            App.DebugLog("");
            MediaHelper.TogglePlayPause();
            IsPlaying = MediaHelper.IsPlaying;
        }

        private void IncrementPosition(object arg)
        {
            string slider = arg as string;
            App.DebugLog(slider);
            if (slider == "A")
            {
                if (startPosition + selectedOffsetOption.Offset < endPosition)
                {
                    StartPosition += SelectedOffsetOption.Offset;
                }
            }
            else if(slider == "B")
            {
                EndPosition = Min(totalDuration, EndPosition + SelectedOffsetOption.Offset);
            }
        }

        private void DecrementPosition(object arg)
        {
            string slider = arg as string;
            App.DebugLog(slider);
            if (slider == "A")
            {
                StartPosition = Max(TimeSpan.Zero, startPosition - SelectedOffsetOption.Offset);
            }
            else if (slider == "B")
            {
                if (endPosition - selectedOffsetOption.Offset > startPosition)
                {
                    EndPosition -= SelectedOffsetOption.Offset;
                }
            }
        }


        private TimeSpan Min(TimeSpan t1, TimeSpan t2)
        {
            if (t1 <= t2) return t1;
            else return t2;
        }

        private TimeSpan Max(TimeSpan t1, TimeSpan t2)
        {
            if (t1 >= t2) return t1;
            else return t2;
        }

        private void JumpToStart()
        {
            MediaHelper.SeekTo(StartPosition);
        }

        public void OnNavigatedFrom(INavigationParameters parameters) { }

        public void OnNavigatedTo(INavigationParameters parameters) { }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            App.DebugLog("");
            var filePathParam = parameters.GetValue<string>(NavigationParameterNames.VideoPath);
            if (!String.IsNullOrEmpty(filePathParam))
            {
                filePath = filePathParam;
            }
        }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog("");
            if (!String.IsNullOrEmpty(filePath))
            {
                await OpenFileAsync();
            }
            MediaHelper.TimeChanged += MediaHelper_TimeChanged;
        }

        protected override Task UnInitializeVMAsync()
        {
            App.DebugLog("");
            MediaHelper.TimeChanged -= MediaHelper_TimeChanged;
            MediaHelper.UnInitMediaPlayer();
            return Task.CompletedTask;
        }

        private string filePath;
        private VideoItem videoItem;
        private readonly IVideoLibrary videoLibrary;
        private readonly INavigationService navigationService;
        private readonly MediaPlayerService playerService;
        private readonly IFFmpegConverter fFmpegConverter;

        private async Task OpenFileAsync()
        {
            NoFileSelected = false;
            videoItem = await videoLibrary.GetVideoItemAsync(filePath);
            TotalDuration = videoItem.Duration;
            var list = await videoLibrary.GetFavoriteScenes(videoItem.VideoId);
            favoriteScenes.AddRange(list);
            MediaHelper.InitMediaPlayer(filePath);
            //await ffmpegService.Test(filePath);
        }

        public async Task ChooseVideoAsync()
        {
            var parameters = new NavigationParameters();
            parameters.Add(NavigationParameterNames.GoBack, true);
            await navigationService.NavigateAsync(PageNames.Videos, parameters);
        }

        public async Task SaveVideoAsync()
        {
            IsConverting = true;
            if (fastTrim)
            {
                await SaveUsingVLCAsync();
            }
            else
            {
                if (saveToGif && SelectedDuration.TotalSeconds < 20)
                {
                    await SaveAsGif();
                }
                else
                {
                    await ConvertVideo();
                }
            }
            IsConverting = false;
        }

        private async Task SaveUsingVLCAsync()
        {
            int start = (int)startPosition.TotalSeconds;
            int end = (int)endPosition.TotalSeconds;
            string startFull = start + "." + startPosition.Milliseconds.ToString("D3");
            string endFull = end + "." + endPosition.Milliseconds.ToString("D3");
            string outputFilename = $@"{videoItem.FileNameWithoutExtension} [{startPosition.ToVideoDuration()}].mp4";
            string outputPath = System.IO.Path.Combine(videoItem.FolderPath, outputFilename);
            var mp = playerService.GetMediaPlayerForTrimming(videoItem.FilePath, outputPath, start, end);
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
            videoLibrary.AddVideo(outputPath);
        }

        private void Mp_EndReached(object sender, EventArgs e)
        {
            App.DebugLog("Video saved");
        }

        private string convertedVideoPath;
        private async Task ConvertVideo()
        {
            int start = (int)startPosition.TotalSeconds;
            string outputFilename = $@"{videoItem.FileNameWithoutExtension} [{startPosition.ToVideoDuration()}].mp4";
            string outputPath = System.IO.Path.Combine(videoItem.FolderPath, outputFilename);
            convertedVideoPath = outputPath;
            var options = new FFmpegToVideoConversionOptions(startPosition, endPosition, videoItem.FilePath, outputPath);
            await fFmpegConverter.CovertToVideo(options);
        }

        private async Task SaveAsGif()
        {
            int start = (int)startPosition.TotalSeconds;
            string outputFilename = $@"{videoItem.FileNameWithoutExtension} [{startPosition.ToVideoDuration()}].gif";
            string outputPath = System.IO.Path.Combine(videoItem.FolderPath, outputFilename);
            var options = new FFmpegToGifConversionOptions(startPosition, endPosition, videoItem.FilePath, outputPath);
            await fFmpegConverter.ConvertToGif(options);
        }
    }

    public class OffsetOption
    {
        public OffsetOption(string name, TimeSpan offset)
        {
            Name = name;
            Offset = offset;
        }

        public string Name { get; set; }
        public TimeSpan Offset { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}