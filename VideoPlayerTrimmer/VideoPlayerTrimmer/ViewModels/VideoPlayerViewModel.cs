using LibVLCSharp.Shared;
using Prism.AppModel;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideoPlayerViewModel : BaseViewModel, INavigationAware, IApplicationLifecycleAware
    {
        private readonly INavigationService navigationService;
        private readonly MediaPlayerService playerService;
        private readonly IVideoLibrary videoLibrary;
        private readonly IVolumeService volumeController;
        private readonly IBrightnessService brightnessController;
        private readonly IOrientationService orientationService;
        private readonly IStatusBarService statusBarService;
        private string filePath;
        private VideoItem videoItem;

        public VideoPlayerViewModel(INavigationService navigationService, MediaPlayerService playerService, 
            IVideoLibrary videoLibrary, IVolumeService volumeController, IBrightnessService brightnessController,
            IOrientationService orientationService, IStatusBarService statusBarService)
        {
            App.DebugLog("");
            this.navigationService = navigationService;
            this.playerService = playerService;
            this.videoLibrary = videoLibrary;
            this.volumeController = volumeController;
            this.brightnessController = brightnessController;
            this.orientationService = orientationService;
            this.statusBarService = statusBarService;
            PlayPauseCommand = new DelegateCommand(() => TogglePlayPause());
            FullScreenCommand = new DelegateCommand(() => EnableFullscreen());
            SeekToCommand = new DelegateCommand<object>((seconds) => SeekTo(seconds));
            ToggleFavoriteCommand = new DelegateCommand(() => ToggleFavorite());
            ToggleControlsVisibilityCommand = new DelegateCommand(() => ToggleControlsVisibility());
            MaxVolume = volumeController.GetMaxVolume();
            Volume = volumeController.GetVolume();
            volumeController.VolumeChanged += VolumeController_VolumeChanged;
            Brightness = Settings.VideoBrightness;
            ApplyBrightness();
        }       

        private MediaPlayer mediaPlayer;
        public MediaPlayer MediaPlayer
        {
            get => mediaPlayer;
            private set => SetProperty(ref mediaPlayer, value);
        }

        public bool IsVideoViewInitialized { get; set; } = false;

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            filePath = parameters.GetValue<string>(NavigationParameterNames.VideoPath);
        }

        public void OnNavigatedTo(INavigationParameters parameters) { }

        public void OnNavigatedFrom(INavigationParameters parameters)
        {
            UnInitMediaPlayer();
        }

        public void OnResume()
        {
            App.DebugLog("");
            InitMediaPlayer();
        }

        public void OnSleep()
        {
            App.DebugLog("");
            UnInitMediaPlayer();
            IsVideoViewInitialized = false;
        }

        public override async Task InitializeAsync()
        {
            App.DebugLog("");
            statusBarService.IsVisible = false;
            orientationService.ChangeToLandscape();
            videoItem = await videoLibrary.GetVideoItemAsync(filePath);
            Title = videoItem.Title;
            if (String.IsNullOrWhiteSpace(Title))
            {
                Title = videoItem.FileName;
            }
            InitMediaPlayer();
        }

        public override Task UninitializeAsync()
        {
            videoLibrary.MarkAsPlayed(videoItem.VideoId);
            statusBarService.IsVisible = true;
            brightnessController.RestoreBrightness();
            orientationService.RestoreOrientation();
            return base.UninitializeAsync();
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer = playerService.GetMediaPlayer(filePath);
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.Playing += MediaPlayer_Playing1;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
          
        }

        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            OnPlaybackStateChange(false);
        }

        private void MediaPlayer_Playing1(object sender, EventArgs e)
        {
            OnPlaybackStateChange(true);
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer.Pause();
            lastPosition = MediaPlayer.Position;
            MediaPlayer.Stop();

            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.Playing -= MediaPlayer_Playing1;
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.EndReached -= MediaPlayer_EndReached;
            MediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
        }

        public DelegateCommand PlayPauseCommand { get; }
        public DelegateCommand FullScreenCommand { get; }
        public DelegateCommand<object> SeekToCommand { get;}
        public DelegateCommand ToggleFavoriteCommand { get; }
        public DelegateCommand ToggleControlsVisibilityCommand { get; }

        private float lastPosition = 0;
        private bool isPausedByUser = false;

        private void TogglePlayPause()
        {
            App.DebugLog("");
            if (MediaPlayer.IsPlaying)
            {
                Pause();
                isPausedByUser = true;
            }
            else
            {
                Play();
                isPausedByUser = false;
            }
        }

        public void StartPlayingOrResume()
        {
            App.DebugLog("");
            MediaPlayer.Playing += MediaPlayer_Playing;

            MediaPlayer.Play();
            MediaPlayer.Position = lastPosition;
        }

        private async void MediaPlayer_Playing(object sender, EventArgs e)
        {
            App.DebugLog("");
            if (isPausedByUser)
            {
                // If Pause() is called without delay, videoview is black
                MediaPlayer.Mute = true;
                await Task.Delay(100);
                MediaPlayer.Pause();
                MediaPlayer.Mute = false;
            }
            MediaPlayer.Playing -= MediaPlayer_Playing;
            //AfterPlaybackStateChange();
            TotalTime = TimeSpan.FromMilliseconds(MediaPlayer.Length);
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            CurrentTime = TimeSpan.FromMilliseconds(e.Time);
        }

        private void Play()
        {
            App.DebugLog("");
            if (IsVideoViewInitialized)
            {
                App.DebugLog("VideViewInitialized");
                MediaPlayer.Play();
            }
        }

        private void Pause()
        {
            App.DebugLog("");
            MediaPlayer.Pause();
        }

        private void Next() { }

        private void Previous() { }

        private void EnableFullscreen() { }

        private void OnPlaybackStateChange(bool isPlaying)
        {
            App.DebugLog(isPlaying.ToString());
            if (isPlaying)
            {
                PlayPauseIcon = "ep-controller-paus";
            }
            else
            {
                PlayPauseIcon = "ep-controller-play";
            }
        }

        #region VolumeAndBrightness

        private string playPauseIcon = "ep-controller-play";
        public string PlayPauseIcon
        {
            get { return playPauseIcon; }
            set { SetProperty(ref playPauseIcon, value); }
        }

        private int volume = 0;
        public int Volume
        {
            get { return volume; }
            set
            {
                SetProperty(ref volume, value);
            }
        }

        public void ApplyVolume()
        {
            App.DebugLog(volume.ToString());
            volumeController.SetVolume(volume);
        }

        private int maxVolume = 10;
        public int MaxVolume
        {
            get { return maxVolume; }
            set { SetProperty(ref maxVolume, value); }
        }

        private bool isVolumeIndicatorVisible = false;
        public bool IsVolumeIndicatorVisible
        {
            get { return isVolumeIndicatorVisible; }
            set { SetProperty(ref isVolumeIndicatorVisible, value); }
        }

        private async void VolumeController_VolumeChanged(object sender, VolumeChangedEventArgs e)
        {
            await Task.Delay(500);
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                Volume = volumeController.GetVolume();
            });
        }

        private int brightness;
        public int Brightness
        {
            get { return brightness; }
            set
            {
                SetProperty(ref brightness, value);
                Settings.VideoBrightness = value;
            }
        }

        public int MaxBrightness { get; } = 15;

        public void ApplyBrightness()
        {
            App.DebugLog(brightness.ToString());
            brightnessController.SetBrightness(((float)brightness) / (float)MaxBrightness);
        }

        private bool isBrightnessIndicatorVisible = false;
        public bool IsBrightnessIndicatorVisible
        {
            get { return isBrightnessIndicatorVisible; }
            set { SetProperty(ref isBrightnessIndicatorVisible, value); }
        }

        #endregion

        private TimeSpan currentTime = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get { return currentTime; }
            set { SetProperty(ref currentTime, value); }
        }

        private TimeSpan totalTime = TimeSpan.Zero;
        public TimeSpan TotalTime
        {
            get { return totalTime; }
            set { SetProperty(ref totalTime, value); }
        }

        private string title = "";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private bool isFavorite = false;
        public bool IsFavorite
        {
            get { return isFavorite; }
            set { SetProperty(ref isFavorite, value); }
        }

        private bool showControls;
        public bool ShowControls
        {
            get { return showControls; }
            set { SetProperty(ref showControls, value); }
        }

        private void ToggleControlsVisibility()
        {
            ShowControls = !ShowControls;
        }

        private void ToggleFavorite()
        {
            IsFavorite = !IsFavorite;
        }

        public void SeekTo(object value)
        {
            if (value == null) return;
            double seconds = (double)value;
            if (MediaPlayer.IsSeekable)
            {
                long newTime = (long)(seconds * 1000);
                if (newTime > MediaPlayer.Length)
                {
                    MediaPlayer.Time = newTime - 100;
                }
                else
                {
                    MediaPlayer.Time = newTime;
                }
            }
        }
    }
}
