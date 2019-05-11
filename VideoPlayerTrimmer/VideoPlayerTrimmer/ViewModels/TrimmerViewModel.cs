using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class TrimmerViewModel : BaseViewModel, INavigationAware
    {
        public TrimmerViewModel(IVideoLibrary videoLibrary, INavigationService navigationService)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            this.navigationService = navigationService;

            MediaHelper = new MediaPlayerHelper();
            MediaHelper.IsPausedByUser = true;
            MediaHelper.MediaPlayerReady += MediaPlayerHelper_MediaPlayerReady;
            MediaHelper.PlaybackStateChanged += MediaHelper_PlaybackStateChanged;

            GoToNextFavSceneCommand = new DelegateCommand(GoToNextFavScene);
            GoToPrevFavSceneCommand = new DelegateCommand(GoToPrevFavScene);
            IncrementPositionCommand = new DelegateCommand<object>((e) => IncrementPosition(e));
            DecrementPositionCommand = new DelegateCommand<object>((e) => DecrementPosition(e));
            JumpToStartCommand = new DelegateCommand(() => JumpToStart());
            TogglePlayPauseCommand = new DelegateCommand(TogglePlayPause);

            OffsetOptions = new List<OffsetOption>()
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

        private void MediaHelper_TimeChanged(object sender, LibVLCSharp.Shared.MediaPlayerTimeChangedEventArgs e)
        {
            var time = TimeSpan.FromMilliseconds(e.Time);
            CurrentPosition = time;
        }

        private void MediaPlayerHelper_MediaPlayerReady(object sender, EventArgs e)
        {
            var t = TimeSpan.FromMilliseconds(MediaHelper.MediaPlayer.Length);
            if(t.TotalMilliseconds!= totalDuration.TotalMilliseconds)
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
                //if (MediaHelper.MediaPlayer != null)
                //{
                //    MediaHelper.MediaPlayer.Media.AddOption(":stop-time=" + (int)EndPosition.TotalSeconds);
                //}
            }
        }

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

        public List<OffsetOption> OffsetOptions { get; }

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
            filePath = parameters.GetValue<string>(NavigationParameterNames.VideoPath);
        }

        public override async Task OnAppearingAsync(bool firstTime)
        {
            App.DebugLog("");
            if (!String.IsNullOrEmpty(filePath))
            {
                await OpenFileAsync();
            }
        }

        public override Task OnDisappearingAsync(bool firstTime)
        {
            App.DebugLog("");
            MediaHelper.UnInitMediaPlayer();
            return Task.CompletedTask;
        }

        private string filePath;
        private VideoItem videoItem;
        private readonly IVideoLibrary videoLibrary;
        private readonly INavigationService navigationService;

        private async Task OpenFileAsync()
        {
            NoFileSelected = false;
            videoItem = await videoLibrary.GetVideoItemAsync(filePath);
            TotalDuration = videoItem.Duration;
            var list = await videoLibrary.GetFavoriteScenes(videoItem.VideoId);
            favoriteScenes.AddRange(list);
            MediaHelper.InitMediaPlayer(filePath);
            MediaHelper.TimeChanged += MediaHelper_TimeChanged;
            MediaHelper.MediaPlayer.Media.AddOption(":repeat");
        }

        public async Task ChooseVideoAsync()
        {
            var parameters = new NavigationParameters();
            parameters.Add(NavigationParameterNames.GoBack, true);
            await navigationService.NavigateAsync(PageNames.Videos, parameters);
        }

        public async Task SaveVideoAsync()
        {

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