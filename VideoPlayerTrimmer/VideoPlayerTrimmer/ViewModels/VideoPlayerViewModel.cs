using LibVLCSharp.Shared;
using Prism.AppModel;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

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
            SeekToCommand = new DelegateCommand<object>((seconds) => SeekTo(seconds));
            ToggleFavoriteCommand = new DelegateCommand(() => ToggleFavorite());
            ToggleControlsVisibilityCommand = new DelegateCommand(() => ToggleControlsVisibility());
            MaxVolume = volumeController.GetMaxVolume();
            Volume = volumeController.GetVolume();
            volumeController.VolumeChanged += VolumeController_VolumeChanged;
            Brightness = Settings.VideoBrightness;
            favoriteScenes = new FavoritesCollection(favoriteSceneDuration);
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

        public void OnNavigatedFrom(INavigationParameters parameters) { }

        public void OnResume()
        {
            App.DebugLog("");
            statusBarService.IsVisible = false;
            orientationService.ChangeToLandscape();
            ApplyBrightness();
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
            ApplyBrightness();
            videoItem = await videoLibrary.GetVideoItemAsync(filePath);
            Title = videoItem.Title;
            if (String.IsNullOrWhiteSpace(Title))
            {
                Title = videoItem.FileName;
            }
            lastPosition = (long)videoItem.Preferences.Position.TotalMilliseconds;
            var favScenes = await videoLibrary.GetFavoriteScenes(videoItem.VideoId);
            favoriteScenes = new FavoritesCollection(favoriteSceneDuration, favScenes);
            InitMediaPlayer();
        }

        public override async Task UninitializeAsync()
        {
            App.DebugLog("");
            UnInitMediaPlayer();
            await videoLibrary.MarkAsPlayedAsync(videoItem);
            videoItem.Preferences.Position = CurrentTime;
            await videoLibrary.SaveVideoItemPreferences(videoItem);
            await videoLibrary.SaveFavoriteScenes(videoItem.VideoId, favoriteScenes.Select(s => s.Value));
            statusBarService.IsVisible = true;
            brightnessController.RestoreBrightness();
            orientationService.RestoreOrientation();
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
            MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer.Pause();
            lastPosition = MediaPlayer.Time;
            MediaPlayer.Stop();

            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.Playing -= MediaPlayer_Playing1;
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.EndReached -= MediaPlayer_EndReached;
            MediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
            MediaPlayer.SnapshotTaken -= MediaPlayer_SnapshotTaken;
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_SnapshotTaken(object sender, MediaPlayerSnapshotTakenEventArgs e)
        {
            App.DebugLog(e.Filename);
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            OnPlaybackStateChange(false);
        }

        private void MediaPlayer_Playing1(object sender, EventArgs e)
        {
            OnPlaybackStateChange(true);
        }

        public DelegateCommand PlayPauseCommand { get; }
        public DelegateCommand<object> SeekToCommand { get;}
        public DelegateCommand ToggleFavoriteCommand { get; }
        public DelegateCommand ToggleControlsVisibilityCommand { get; }

        private long lastPosition = 0;
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
            MediaPlayer.Time = lastPosition;
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
            ShowIsFavorite(currentTime);
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


        private TimeSpan favoriteSceneDuration = TimeSpan.FromSeconds(5);
        private FavoritesCollection favoriteScenes;

        private void ShowIsFavorite(TimeSpan position)
        {
            if (favoriteScenes.IsFavorite(position))
            {
                IsFavorite = true;
            }
            else
            {
                IsFavorite = false;
            }
        }

        private void ToggleFavorite()
        {
            if (IsFavorite)
            {
                favoriteScenes.RemoveFavorite(currentTime);
            }
            else
            {
                string fileName = GetHashString(videoItem.FilePath + currentTime.ToString());
                string path = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, Configuration.SnaphotsFolderName, fileName);

                favoriteScenes.AddFavorite(new FavoriteScene()
                {
                    Position = currentTime,
                    ThumbnailPath = "",
                    SnapshotPath = path
                });
                MediaPlayer.TakeSnapshot(0, path, 0, 0);
            }
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


        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }

    public class FavoritesCollection : Dictionary<TimeSpan, FavoriteScene>
    {
        public FavoritesCollection(TimeSpan interval)
        {
            Interval = interval;
        }

        public FavoritesCollection(TimeSpan interval, IEnumerable<FavoriteScene> scenes)
        {
            Interval = interval;
            foreach(var scene in scenes)
            {
                this.AddFavorite(scene);
            }
        }

        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(5);

        private TimeSpan FindIntervalBeginning(TimeSpan value)
        {
            var seconds = Interval.TotalSeconds * Math.Floor(value.TotalSeconds / Interval.TotalSeconds);
            return TimeSpan.FromSeconds(seconds);
        }
        
        public void AddFavorite(FavoriteScene scene)
        {
            var key = FindIntervalBeginning(scene.Position);
            this[key] = scene;
        }

        public void RemoveFavorite(TimeSpan position)
        {
            var key = FindIntervalBeginning(position);
            this.Remove(key);
        }

        public bool IsFavorite(TimeSpan position)
        {
            var key = FindIntervalBeginning(position);
            return this.ContainsKey(key);
        }
    }
}
