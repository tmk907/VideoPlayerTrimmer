using LibVLCSharp.Forms.Shared;
using LibVLCSharp.Shared;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideoPlayerViewModel : BaseViewModel   , IInitialize
    {
        private readonly MediaPlayerService playerService;
        private readonly IVideoLibrary videoLibrary;
        private readonly IVolumeService volumeController;
        private readonly IBrightnessService brightnessController;
        private readonly IOrientationService orientationService;
        private readonly IStatusBarService statusBarService;
        private string filePath;
        private VideoItem videoItem;

        public VideoPlayerViewModel(MediaPlayerService playerService, 
            IVideoLibrary videoLibrary, IVolumeService volumeController, IBrightnessService brightnessController,
            IOrientationService orientationService, IStatusBarService statusBarService)
        {
            App.DebugLog("");
            this.playerService = playerService;
            this.videoLibrary = videoLibrary;
            this.volumeController = volumeController;
            this.brightnessController = brightnessController;
            this.orientationService = orientationService;
            this.statusBarService = statusBarService;
            ToggleFavoriteCommand = new DelegateCommand(ToggleFavorite);
            ToggleControlsVisibilityCommand = new DelegateCommand(ToggleControlsVisibility);
            ToggleAudioTracksCommand = new DelegateCommand(ToggleAudioTracks);
            ToggleSubtitlesCommand = new DelegateCommand(ToggleSubtitles);
            ToggleMediaInfoCommand = new DelegateCommand(ToggleMediaInfo);
            SelectSubtitlesCommand = new DelegateCommand<object>(SelectSubtitles, (e) => canChangeSubtitles);
            SelectAudioTrackCommand = new DelegateCommand<object>(SelectAudioTrack, (e) => canChangeAudioTrack);
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

        public LibVLC LibVLC
        {
            get => playerService.LibVLC.Value;
        }


        public bool IsVideoViewInitialized { get; set; } = false;

        public void Initialize(INavigationParameters parameters)
        {
            filePath = parameters.GetValue<string>(NavigationParameterNames.VideoPath);
            if (parameters.ContainsKey(NavigationParameterNames.Position))
            {
                var pos = parameters.GetValue<TimeSpan>(NavigationParameterNames.Position);
                userPosition = (long)pos.TotalMilliseconds;
            }
        }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog(firstTimeAppeared.ToString());
            if (firstTimeAppeared)
            {
                videoItem = await videoLibrary.GetVideoItemAsync(filePath);
                Title = videoItem.Title;
                if (String.IsNullOrWhiteSpace(Title))
                {
                    Title = videoItem.FileName;
                }
                if (Settings.ResumePlayback)
                {
                    lastPosition = (long)videoItem.Preferences.Position.TotalMilliseconds;
                }
                else
                {
                    lastPosition = 0;
                }
                var favScenes = await videoLibrary.GetFavoriteScenes(videoItem.VideoId);
                favoriteScenes = new FavoritesCollection(favoriteSceneDuration, favScenes);
            }

            statusBarService.IsVisible = false;
            orientationService.ChangeToLandscape();
            ApplyBrightness();
            
            if (userPosition != 0)
            {
                lastPosition = userPosition;
            }

            InitMediaPlayer();
            StartPlayingOrResume();
        }

        protected override async Task UnInitializeVMAsync()
        {
            App.DebugLog(firstTimeDisappeared.ToString());
            UnInitMediaPlayer();

            statusBarService.IsVisible = true;
            brightnessController.RestoreBrightness();
            orientationService.RestoreOrientation();

            await videoLibrary.MarkAsPlayedAsync(videoItem);
            videoItem.Preferences.Position = CurrentTime;
            if (currentTime >= totalTime - TimeSpan.FromSeconds(0.5))
            {
                videoItem.Preferences.Position = TimeSpan.Zero;
            }
            await videoLibrary.SaveVideoItemPreferences(videoItem);
            await videoLibrary.SaveFavoriteScenes(videoItem.VideoId, favoriteScenes.Select(s => s.Value));
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer = playerService.GetMediaPlayer(filePath);

            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
            
            IsVideoViewInitialized = true;
        }

        private void MediaPlayer_PositionChanged(object sender, MediaPlayerPositionChangedEventArgs e)
        {
            App.DebugLog("PositionChanged: " + e.Position);
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");
            IsVideoViewInitialized = false;

            MediaPlayer.Pause();
            lastPosition = MediaPlayer.Time;
            MediaPlayer.Stop();

            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged -= MediaPlayer_LengthChanged;
            MediaPlayer.Playing -= MediaPlayer_Playing;
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.EndReached -= MediaPlayer_EndReached;
            MediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
            MediaPlayer.SnapshotTaken -= MediaPlayer_SnapshotTaken;
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            var state = MediaPlayer?.State;
            var length = MediaPlayer == null ||
                state == VLCState.Ended || state == VLCState.Error || state == VLCState.NothingSpecial ||
                state == VLCState.Stopped ? 0 : mediaPlayer.Length;
            App.DebugLog(TimeSpan.FromMilliseconds(length).ToString());
            Device.BeginInvokeOnMainThread(() =>
            {
                TotalTime = TimeSpan.FromMilliseconds(length);
            });
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            //App.DebugLog("TimeChanged " + e.Time);
            CurrentTime = TimeSpan.FromMilliseconds(e.Time);
            ShowIsFavorite(currentTime);
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
            App.DebugLog("");
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        public DelegateCommand ToggleFavoriteCommand { get; }
        public DelegateCommand ToggleControlsVisibilityCommand { get; }
        public DelegateCommand ToggleAudioTracksCommand { get; }
        public DelegateCommand ToggleSubtitlesCommand { get; }
        public DelegateCommand ToggleMediaInfoCommand { get; }
        public DelegateCommand<object> SelectSubtitlesCommand { get; }
        public DelegateCommand<object> SelectAudioTrackCommand { get; }


        private long lastPosition = 0;
        private long userPosition = 0;
        private bool isPausedByUser = false;

        public void StartPlayingOrResume()
        {
            App.DebugLog("");
            if (isPausedByUser)
            {

            }
            else
            {
                MediaPlayer.Play();
            }
            MediaPlayer.Time = lastPosition;
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
            if (IsPopupVisible)
            {
                IsAudioTracksPopupVisible = false;
                IsSubtitlesPopupVisible = false;
                IsMediaInfoPopupVisible = false;
                ShowControls = false;
            }
            else
            {
                ShowControls = !ShowControls;
            }
        }

        private bool IsPopupVisible
        {
            get { return IsAudioTracksPopupVisible || IsSubtitlesPopupVisible || IsMediaInfoPopupVisible; }
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
            double seconds = Convert.ToDouble(value);
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

        #region Audio,subtitles,info

        private bool isAudioTracksPopupVisible = false;
        public bool IsAudioTracksPopupVisible
        {
            get { return isAudioTracksPopupVisible; }
            set { SetProperty(ref isAudioTracksPopupVisible, value); }
        }

        public ObservableCollection<AudioTrackInfo> AudioTracks { get; } = new ObservableCollection<AudioTrackInfo>();

        private bool canChangeAudioTrack = false;

        private void SelectAudioTrack(object audioTrackInfo)
        {
            canChangeAudioTrack = false;
            var selected = (AudioTrackInfo)audioTrackInfo;
            foreach (var audio in AudioTracks)
            {
                audio.IsSelected = false;
            }
            selected.IsSelected = true;
            MediaPlayer.SetAudioTrack(selected.Id);
            canChangeAudioTrack = true;
        }

        public void ToggleAudioTracks()
        {
            if (AudioTracks.Count == 0)
            {
                foreach(var item in MediaPlayer.AudioTrackDescription)
                {
                    AudioTracks.Add(new AudioTrackInfo() { Id = item.Id, Name = item.Name });
                }
                AudioTracks.SingleOrDefault(a => a.Id == MediaPlayer.AudioTrack).IsSelected = true;
            }
            IsAudioTracksPopupVisible = !IsAudioTracksPopupVisible;
        }

        private bool isSubtitlesPopupVisible = false;
        public bool IsSubtitlesPopupVisible
        {
            get { return isSubtitlesPopupVisible; }
            set { SetProperty(ref isSubtitlesPopupVisible, value); }
        }

        public ObservableCollection<SubtitleInfo> Subtitles { get; } = new ObservableCollection<SubtitleInfo>();

        private bool canChangeSubtitles = true;

        private void SelectSubtitles(object subtitleInfo)
        {
            canChangeSubtitles = false;
            var selected = (SubtitleInfo)subtitleInfo;
            foreach (var sub in Subtitles)
            {
                sub.IsSelected = false;
            }
            selected.IsSelected = true;
            MediaPlayer.SetSpu(selected.Id);
            canChangeSubtitles = true;
        }

        public void ToggleSubtitles()
        {
            if (Subtitles.Count == 0)
            {
                foreach (var item in MediaPlayer.SpuDescription)
                {
                    Subtitles.Add(new SubtitleInfo() { Id = item.Id, Name = item.Name });
                }
                var sub = Subtitles.SingleOrDefault(a => a.Id == MediaPlayer.Spu);
                if (sub != null)
                {
                    sub.IsSelected = true;
                }
            }
            IsSubtitlesPopupVisible = !IsSubtitlesPopupVisible;
        }

        private bool isMediaInfoPopupVisible = false;
        public bool IsMediaInfoPopupVisible
        {
            get { return isMediaInfoPopupVisible; }
            set { SetProperty(ref isMediaInfoPopupVisible, value); }
        }

        private MediaInfo mediaInfo;
        public MediaInfo MediaInfo
        {
            get { return mediaInfo; }
            set { SetProperty(ref mediaInfo, value); }
        }

        public void ToggleMediaInfo()
        {
            IsMediaInfoPopupVisible = !IsMediaInfoPopupVisible;
            if (IsMediaInfoPopupVisible && mediaInfo == null)
            {
                var mi = new MediaInfo();
                mi.FileName = videoItem.FileName;
                mi.FilePath = videoItem.FilePath;
                mi.VideoTitle = title;
                mi.Fps = MediaPlayer.Fps;
                TimeSpan.FromMilliseconds(MediaPlayer.Length);
                mi.AudioTracks = MediaPlayer.AudioTrackDescription.Where(s => s.Id != -1)
                    .Select(a => new AudioTrackInfo() { Id = a.Id, Name = a.Name }).ToList();
                mi.Subtitles = MediaPlayer.SpuDescription.Where(s => s.Id != -1)
                    .Select(s => new SubtitleInfo() { Id = s.Id, Name = s.Name }).ToList();
                uint width = 0;
                uint height = 0;
                MediaPlayer.Size(0, ref width, ref height);
                mi.Resolution = width + "x" + height;
                MediaInfo = mi;
            }
        }

        #endregion

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
