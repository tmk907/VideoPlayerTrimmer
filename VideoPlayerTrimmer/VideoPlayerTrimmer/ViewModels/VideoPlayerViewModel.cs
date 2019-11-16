using AsyncAwaitBestPractices.MVVM;
using LibVLCSharp.Shared;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.PlayerControls;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideoPlayerViewModel : BaseViewModel
    {
        private readonly IVideoLibrary videoLibrary;
        private readonly IVolumeService volumeController;
        private readonly IBrightnessService brightnessController;
        private readonly IOrientationService orientationService;
        private readonly IStatusBarService statusBarService;
        private string filePath;
        private VideoItem videoItem;

        public VideoPlayerViewModel(MediaPlayerBuilder playerService, 
            IVideoLibrary videoLibrary, IVolumeService volumeController, IBrightnessService brightnessController,
            IOrientationService orientationService, IStatusBarService statusBarService)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            this.volumeController = volumeController;
            this.brightnessController = brightnessController;
            this.orientationService = orientationService;
            this.statusBarService = statusBarService;
            ToggleFavoriteCommand = new Command(ToggleFavorite);
            ToggleControlsVisibilityCommand = new Command(ToggleControlsVisibility);
            ToggleAudioTracksCommand = new Command(ToggleAudioTracks);
            ToggleSubtitlesCommand = new Command(ToggleSubtitles);
            ToggleMediaInfoCommand = new Command(ToggleMediaInfo);
            SelectSubtitlesCommand = new Command<object>(SelectSubtitles, (e) => canChangeSubtitles);
            SelectAudioTrackCommand = new Command<object>(SelectAudioTrack, (e) => canChangeAudioTrack);
            AddSubtitlesFromFileCommand = new AsyncCommand(AddSubtitlesFromFile);
            MaxVolume = volumeController.GetMaxVolume();
            Volume = volumeController.GetVolume();
            volumeController.VolumeChanged += VolumeController_VolumeChanged;
            Brightness = Settings.VideoBrightness;
            favoriteScenes = new FavoritesCollection(favoriteSceneDuration);

            VlcPlayerHelper = new VlcPlayerHelper(playerService);
        }

        private VlcPlayerHelper vlcPlayerHelper;
        public VlcPlayerHelper VlcPlayerHelper
        {
            get { return vlcPlayerHelper; }
            set { vlcPlayerHelper = value; }
        }

        public override void OnNavigating(Dictionary<string, string> navigationArgs)
        {
            base.OnNavigating(navigationArgs);
            
            filePath = navigationParameters[NavigationParameterNames.VideoPath];
            if (navigationParameters.ContainsKey(NavigationParameterNames.Position))
            {
                var pos = TimeSpan.Parse(navigationParameters[NavigationParameterNames.Position]);
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
            lastPosition = lastPosition > 750 ? lastPosition : 0;

            InitMediaPlayer();
            StartPlayingOrResume();
        }

        protected override async Task UnInitializeVMAsync()
        {
            App.DebugLog(firstTimeDisappeared.ToString());
            
            var currentTime = vlcPlayerHelper.ElapsedTime;
            videoItem.Preferences.Position = currentTime;
            if (currentTime >= vlcPlayerHelper.TotalTime - TimeSpan.FromSeconds(0.5))
            {
                videoItem.Preferences.Position = TimeSpan.Zero;
            }

            UnInitMediaPlayer();

            statusBarService.IsVisible = true;
            brightnessController.RestoreBrightness();
            orientationService.RestoreOrientation();

            await videoLibrary.MarkAsPlayedAsync(videoItem);
            await videoLibrary.SaveVideoItemPreferences(videoItem);
            await videoLibrary.SaveFavoriteScenes(videoItem.VideoId, favoriteScenes.Select(s => s.Value));
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");
            VlcPlayerHelper.LoadFile(filePath);

            VlcPlayerHelper.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            VlcPlayerHelper.MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");

            VlcPlayerHelper.MediaPlayer.Pause();
            VlcPlayerHelper.MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            VlcPlayerHelper.MediaPlayer.SnapshotTaken -= MediaPlayer_SnapshotTaken;
            VlcPlayerHelper.OnDisappearing();
        }
        
        private void MediaPlayer_SnapshotTaken(object sender, MediaPlayerSnapshotTakenEventArgs e)
        {
            App.DebugLog(e.Filename);
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            ShowIsFavorite(TimeSpan.FromMilliseconds(e.Time));
        }

        private void PlaybackControls_OnSubtitlesButtonClicked(object sender, EventArgs e)
        {
            ToggleSubtitles();
        }

        public Command ToggleFavoriteCommand { get; }
        public Command ToggleControlsVisibilityCommand { get; }
        public Command ToggleAudioTracksCommand { get; }
        public Command ToggleSubtitlesCommand { get; }
        public Command ToggleMediaInfoCommand { get; }
        public Command<object> SelectSubtitlesCommand { get; }
        public Command<object> SelectAudioTrackCommand { get; }
        public IAsyncCommand AddSubtitlesFromFileCommand { get; }


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
                VlcPlayerHelper.MediaPlayer.Play();
            }
            if (lastPosition > 0)
            {
                VlcPlayerHelper.MediaPlayer.Time = lastPosition;
            }
        }

        #region VolumeAndBrightness

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


        private bool isControlVisible = true;
        public bool IsControlVisible
        {
            get { return isControlVisible; }
            set { SetProperty(ref isControlVisible, value); }
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
                IsControlVisible = false;
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
            bool isCurrentSceneFavorite = favoriteScenes.IsFavorite(position);

            if (isCurrentSceneFavorite != isFavorite)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    IsFavorite = isCurrentSceneFavorite;
                });
            }
        }

        private void ToggleFavorite()
        {
            var currentTime = vlcPlayerHelper.ElapsedTime;
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
                VlcPlayerHelper.MediaPlayer.TakeSnapshot(0, path, 0, 0);
            }
            IsFavorite = !IsFavorite;
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
            VlcPlayerHelper.SetAudioTrack(selected.Id);
            canChangeAudioTrack = true;
        }

        public void ToggleAudioTracks()
        {
            if (AudioTracks.Count == 0)
            {
                AudioTracks.AddRange(VlcPlayerHelper.GetAudioTracks());
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
            vlcPlayerHelper.SetSubtitles(selected.VlcId);
            canChangeSubtitles = true;
        }

        public void ToggleSubtitles()
        {
            if (Subtitles.Count == 0)
            {
                Subtitles.AddRange(vlcPlayerHelper.GetSubtitleTracks());
            }
            IsSubtitlesPopupVisible = !IsSubtitlesPopupVisible;
        }

        private async Task AddSubtitlesFromFile()
        {
            try
            {
                FileData fileData = await CrossFilePicker.Current.PickFile();
                if (fileData == null) return; // user canceled file picking

                foreach (var sub in Subtitles)
                {
                    sub.IsSelected = false;
                }
                var subs = Subtitles.FirstOrDefault(x => x.FilePath == fileData.FilePath);
                if (subs == null)
                {
                    Subtitles.Add(new SubtitleInfo()
                    {
                        FilePath = fileData.FilePath,
                        Name = fileData.FileName
                    });
                }
                else
                {
                    subs.IsSelected = true;
                }
                vlcPlayerHelper.SetSubtitles(fileData.FilePath);
            }
            catch (Exception ex)
            {
                App.DebugLog(ex.ToString());
            }
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
                mi.Fps = VlcPlayerHelper.MediaPlayer.Fps;
                TimeSpan.FromMilliseconds(VlcPlayerHelper.MediaPlayer.Length);
                mi.AudioTracks = VlcPlayerHelper.MediaPlayer.AudioTrackDescription.Where(s => s.Id != -1)
                    .Select(a => new AudioTrackInfo() { Id = a.Id, Name = a.Name }).ToList();
                mi.Subtitles = VlcPlayerHelper.MediaPlayer.SpuDescription.Where(s => s.Id != -1)
                    .Select(s => new SubtitleInfo() { VlcId = s.Id, Name = s.Name }).ToList();
                uint width = 0;
                uint height = 0;
                VlcPlayerHelper.MediaPlayer.Size(0, ref width, ref height);
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
