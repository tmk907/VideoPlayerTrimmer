using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.FilePicker;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.PlayerControls;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class TrimmerViewModel : BaseViewModel
    {
        public TrimmerViewModel(IVideoLibrary videoLibrary, MediaPlayerBuilder playerService, ConverterHelper converterHelper)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            this.playerService = playerService;
            _converterHelper = converterHelper;
            _converterHelper.ConversionStarted += ConversionStarted;
            _converterHelper.ConversionEnded += ConversionEnded;

            VlcPlayerHelper = new VlcPlayerHelper(playerService);
            vlcPlayerHelper.PlayerReady += VlcPlayerHelper_PlayerReady;

            GoToNextFavSceneCommand = new Command(GoToNextFavScene);
            GoToPrevFavSceneCommand = new Command(GoToPrevFavScene);
            IncrementPositionCommand = new Command<object>((e) => IncrementPosition(e));
            DecrementPositionCommand = new Command<object>((e) => DecrementPosition(e));
            JumpToStartCommand = new Command(() => JumpToStart());

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

        private void ConversionStarted()
        {
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                IsConverting = true;
            });
        }

        private void ConversionEnded()
        {
            Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
            {
                IsConverting = false;
            });
        }
        
        private void VlcPlayerHelper_PlayerReady()
        {
            if (vlcPlayerHelper.MediaPlayer.Fps > 0)
            {
                var frameDuration = 1.0 / vlcPlayerHelper.MediaPlayer.Fps;
                Device.BeginInvokeOnMainThread(() =>
                {
                    var frameOption = OffsetOptions.FirstOrDefault(o => o.Name == "1 Frame");
                    if (frameOption != null)
                    {
                        OffsetOptions.Remove(frameOption);
                    }
                    OffsetOptions.Add(new OffsetOption("1 Frame", TimeSpan.FromSeconds(frameDuration)));
                });
            }
        }

        private VlcPlayerHelper vlcPlayerHelper;
        public VlcPlayerHelper VlcPlayerHelper
        {
            get { return vlcPlayerHelper; }
            set { vlcPlayerHelper = value; }
        }

        private void MediaPlayer_TimeChanged(object sender, LibVLCSharp.Shared.MediaPlayerTimeChangedEventArgs e)
        {
            var time = TimeSpan.FromMilliseconds(e.Time);
            CurrentPosition = time;
        }

        private TimeSpan startPosition = TimeSpan.Zero;
        public TimeSpan StartPosition
        {
            get { return startPosition; }
            set
            {
                SetProperty(ref startPosition, value);
                vlcPlayerHelper.SeekTo(startPosition);
            }
        }

        private TimeSpan endPosition;
        public TimeSpan EndPosition
        {
            get { return endPosition; }
            set
            {
                SetProperty(ref endPosition, value);
                vlcPlayerHelper.SeekTo(endPosition);
            }
        }

        public TimeSpan SelectedDuration { get => endPosition - startPosition; }

        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set { SetProperty(ref currentPosition, value); }
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


        private string outputPath;
        public string OutputPath
        {
            get { return outputPath; }
            set { SetProperty(ref outputPath, value); }
        }

        public Command GoToPrevFavSceneCommand { get; }
        public Command GoToNextFavSceneCommand { get; }
        public Command TogglePlayPauseCommand { get; }
        public Command<object> IncrementPositionCommand { get; }
        public Command<object> DecrementPositionCommand { get; }
        public Command JumpToStartCommand { get; }


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
            vlcPlayerHelper.SeekTo(StartPosition);
        }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog("");

            var dict = App.NavigationService.ParseNavigationParameters(App.NavigationService.BackNavigationParameters);
            if (dict.ContainsKey(NavigationParameterNames.VideoPath))
            {
                filePath = dict[NavigationParameterNames.VideoPath];
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                await OpenFileAsync();
                InitMediaPlayer();
            }
        }

        protected override Task UnInitializeVMAsync()
        {
            App.DebugLog("");
            UnInitMediaPlayer();
            return Task.CompletedTask;
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");
            var startupConfiguration = new StartupConfiguration()
            {
                AutoPlay = false,
                FilePath = filePath
            };
            foreach (var sub in videoItem.SubtitleFiles)
            {
                startupConfiguration.ExternalSubtitles.Add(new SubitlesConfig()
                {
                    FileUrl = sub.FileUrl,
                    Delay = sub.Delay,
                    Encoding = sub.Encoding,
                    IsSelected = sub.IsSelected
                });
            }
            if (videoItem.IsFileSubtitleSelected)
            {
            }
            else
            {
                startupConfiguration.SelectedSubtitlesSpu = videoItem.SelectedSubtitlesId;
            }
            startupConfiguration.EmbeddedSubtitlesDelay = videoItem.EmbeddedSubtitlesDelay;

            VlcPlayerHelper.LoadFile(startupConfiguration);

            VlcPlayerHelper.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");

            if (!String.IsNullOrEmpty(filePath))
            {
                VlcPlayerHelper.MediaPlayer.Pause();
                vlcPlayerHelper.MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
                VlcPlayerHelper.OnDisappearing();
            }
        }

        private string filePath;
        private VideoItem videoItem;
        private readonly IVideoLibrary videoLibrary;
        private readonly MediaPlayerBuilder playerService;
        private readonly ConverterHelper _converterHelper;
        private readonly IFileService _fileService;
        private readonly IFFmpegConverter fFmpegConverter;

        private async Task OpenFileAsync()
        {
            NoFileSelected = false;
            videoItem = await videoLibrary.GetVideoItemAsync(filePath);
            TotalDuration = videoItem.Duration;
            var list = await videoLibrary.GetFavoriteScenes(videoItem.VideoId);
            favoriteScenes.AddRange(list);
            //await ffmpegService.Test(filePath);
        }

        public Task ChooseVideoAsync()
        {
            return App.NavigationService.NavigateToAsync($"{PageNames.TrimmerNav}/{PageNames.Videos}" +
                $"?{NavigationParameterNames.GoBack}={true}");
        }

        public async Task SaveVideoAsync()
        {
            await _converterHelper.ConvertVideoAsync(new ConversionOptions()
            {
                EndPosition = endPosition,
                FastMode = fastTrim,
                FileNameWithoutExtension = videoItem.FileNameWithoutExtension,
                FilePath = videoItem.FilePath,
                SaveAsGif = saveToGif,
                StartPosition = startPosition
            });
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