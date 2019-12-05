using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.FilePicker;
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
        private readonly IOrientationService orientationService;
        private readonly IStatusBarService statusBarService;
        private string filePath;

        public VideoPlayerViewModel(MediaPlayerBuilder playerService, 
            IVideoLibrary videoLibrary, IVolumeService volumeController, IBrightnessService brightnessController,
            IOrientationService orientationService, IStatusBarService statusBarService, IFileService fileService)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            this.orientationService = orientationService;
            this.statusBarService = statusBarService;
            FileService = fileService;
            ToggleFavoriteCommand = new Command(ToggleFavorite);
            ToggleControlsVisibilityCommand = new Command(ToggleControlsVisibility);
            ToggleAudioTracksCommand = new Command(ToggleAudioTracks);
            ToggleSubtitlesCommand = new Command(ToggleSubtitles);
            ToggleMediaInfoCommand = new Command(ToggleMediaInfo);
            SelectSubtitlesCommand = new Command<object>(SelectSubtitles, (e) => canChangeSubtitles);
            SelectAudioTrackCommand = new Command<object>(SelectAudioTrack, (e) => canChangeAudioTrack);
            AddSubtitlesFromFileCommand = new Command(AddSubtitlesFromFile);
            SubtitleFileTappedCommand = new Command<object>(o => SubtitleFileTapped(o));
            CloseFilePickerCommand = new Command(() => { IsSubtitleFilePickerVisible = false; });

            volumeViewModel = new VolumeViewModel(volumeController);
            brightnessViewModel = new BrightnessViewModel(brightnessController);
            
            favoriteScenes = new FavoritesCollection(favoriteSceneDuration);

            VlcPlayerHelper = new VlcPlayerHelper(playerService);
        }
        
        private VideoItem videoItem;
        public VideoItem VideoItem
        {
            get { return videoItem; }
            set { SetProperty(ref videoItem, value); }
        }

        private VolumeViewModel volumeViewModel;
        public VolumeViewModel VolumeViewModel
        {
            get { return volumeViewModel; }
            set { SetProperty(ref volumeViewModel, value); }
        }

        private BrightnessViewModel brightnessViewModel;
        public BrightnessViewModel BrightnessViewModel
        {
            get { return brightnessViewModel; }
            set { SetProperty(ref brightnessViewModel, value); }
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
                    lastPosition = (long)videoItem.Position.TotalMilliseconds;
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
            brightnessViewModel.ApplyBrightness();
            
            if (userPosition != 0)
            {
                lastPosition = userPosition;
            }
            lastPosition = lastPosition > 750 ? lastPosition : 0;

            InitMediaPlayer();
        }

        protected override async Task UnInitializeVMAsync()
        {
            App.DebugLog(firstTimeDisappeared.ToString());
            
            var currentTime = vlcPlayerHelper.ElapsedTime;
            videoItem.Position = currentTime;
            if (currentTime >= vlcPlayerHelper.TotalTime - TimeSpan.FromSeconds(0.5))
            {
                videoItem.Position = TimeSpan.Zero;
            }

            if (videoItem.IsFileSubtitleSelected)
            {
                videoItem.SelectedSubtitlesId = -1;
            }
            else if (Subtitles.Where(x => x.Spu != -1).Any(x => x.IsSelected))
            {
                var spu = Subtitles.First(x => x.IsSelected).Spu;
                videoItem.SelectedSubtitlesId = spu;
            }
            else
            {
                videoItem.SelectedSubtitlesId = -1;
            }

            UnInitMediaPlayer();

            statusBarService.IsVisible = true;
            brightnessViewModel.RestoreBrightness();
            orientationService.RestoreOrientation();

            await videoLibrary.MarkAsPlayedAsync(videoItem);
            await videoLibrary.UpdateVideoItemPreferences(videoItem);
            await videoLibrary.SaveFavoriteScenes(videoItem.VideoId, favoriteScenes.Select(s => s.Value));
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");
            var startupConfiguration = new StartupConfiguration()
            {
                AutoPlay = !isPausedByUser,
                ResumeTime = lastPosition,
                FilePath = filePath
            };
            foreach(var sub in videoItem.SubtitleFiles)
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


        public Command ToggleFavoriteCommand { get; }
        public Command ToggleControlsVisibilityCommand { get; }
        public Command ToggleAudioTracksCommand { get; }
        public Command ToggleSubtitlesCommand { get; }
        public Command ToggleMediaInfoCommand { get; }
        public Command<object> SelectSubtitlesCommand { get; }
        public Command<object> SelectAudioTrackCommand { get; }
        public Command AddSubtitlesFromFileCommand { get; }


        private long lastPosition = 0;
        private long userPosition = 0;
        private bool isPausedByUser = false;

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
            IsAudioTracksPopupVisible = false;
            IsControlVisible = false;
        }

        public void ToggleAudioTracks()
        {
            if (AudioTracks.Count == 0)
            {
                AudioTracks.AddRange(VlcPlayerHelper.GetAudioTracks());
            }
            IsAudioTracksPopupVisible = !IsAudioTracksPopupVisible;
            IsControlVisible = false;
        }

        private bool isSubtitlesPopupVisible = false;
        public bool IsSubtitlesPopupVisible
        {
            get { return isSubtitlesPopupVisible; }
            set { SetProperty(ref isSubtitlesPopupVisible, value); }
        }

        public ObservableCollection<VlcSubtitles> Subtitles { get; } = new ObservableCollection<VlcSubtitles>();

        private bool canChangeSubtitles = true;

        private void SelectSubtitles(object subtitleInfo)
        {
            canChangeSubtitles = false;
            var selected = (VlcSubtitles)subtitleInfo;
            MarkSubtitlesAsSelected(selected);
            vlcPlayerHelper.SetSubtitles(selected);
            canChangeSubtitles = true;
            IsSubtitlesPopupVisible = false;
            IsControlVisible = false;
        }

        public void ToggleSubtitles()
        {
            if (!IsSubtitlesPopupVisible)
            {
                Subtitles.Clear();
                Subtitles.AddRange(vlcPlayerHelper.GetVlcSubtitles());
                if (VideoItem.IsFileSubtitleSelected)
                {
                    SelectedEncoding = VideoItem.SelectedSubtitleFile.Encoding;
                }
                else
                {
                    SelectedEncoding = LibVlcOptions.GetSubtitleEncoding();
                }
            }
            IsSubtitlesPopupVisible = !IsSubtitlesPopupVisible;
            IsControlVisible = false;
        }
        
        private void MarkSubtitlesAsSelected(VlcSubtitles subtitles)
        {
            foreach(var sub in Subtitles)
            {
                sub.IsSelected = false;
            }
            subtitles.IsSelected = true;
            foreach(var sub in videoItem.SubtitleFiles)
            {
                sub.IsSelected = false;
            }
            var selected = videoItem.SubtitleFiles.FirstOrDefault(x => x.FileUrl == subtitles.FileUrl);
            if (selected != null)
            {
                selected.IsSelected = true;
            }
        }

        private void MarkSubtitlesAsSelected(SubtitleFile subtitles)
        {
            foreach (var sub in videoItem.SubtitleFiles)
            {
                sub.IsSelected = false;
            }
            subtitles.IsSelected = true;
        }

        private void AddSubtitlesFromFile()
        {
            StartupPath = videoItem.FolderPath;
            IsSubtitleFilePickerVisible = true;
            IsSubtitlesPopupVisible = false;
            IsControlVisible = false;
        }

        public Command<object> SubtitleFileTappedCommand { get; }
        public Command CloseFilePickerCommand { get; }

        private void SubtitleFileTapped(object item)
        {
            var file = item as FileItem;
            var subs = videoItem.SubtitleFiles.FirstOrDefault(x => x.FilePath == file.Path);
            if (subs == null)
            {
                subs = new SubtitleFile(file.Path, videoItem.VideoId);
                videoItem.SubtitleFiles.Add(subs);
            }
            MarkSubtitlesAsSelected(subs);
            var config = BuildStartupConfiguration();
            vlcPlayerHelper.Reset(config);

            IsSubtitleFilePickerVisible = false;
        }

        private bool isSubtitleFilePickerVisible;
        public bool IsSubtitleFilePickerVisible
        {
            get { return isSubtitleFilePickerVisible; }
            set { SetProperty(ref isSubtitleFilePickerVisible, value); }
        }

        public List<string> EncodingList { get; } = LibVlcOptions.Encoding.All;

        private string selectedEncoding = LibVlcOptions.GetSubtitleEncoding();
        public string SelectedEncoding
        {
            get { return selectedEncoding; }
            set 
            {
                if (value != selectedEncoding)
                {
                    if (videoItem.IsFileSubtitleSelected)
                    {
                        videoItem.SelectedSubtitleFile.Encoding = value;
                        var config = BuildStartupConfiguration();
                        vlcPlayerHelper.Reset(config);
                    }
                }
                SetProperty(ref selectedEncoding, value); 
            }
        }

        private StartupConfiguration BuildStartupConfiguration()
        {
            var config = new StartupConfiguration()
            {
                AutoPlay = vlcPlayerHelper.IsPlaying,
                EmbeddedSubtitlesDelay = videoItem.EmbeddedSubtitlesDelay,
                EmbeddedSubtitlesEncoding = LibVlcOptions.GetSubtitleEncoding(),
                FilePath = videoItem.FilePath,
                ResumeTime = (long)vlcPlayerHelper.ElapsedTime.TotalMilliseconds,
            };
            config.ExternalSubtitles.AddRange(videoItem.SubtitleFiles.Select(x => new SubitlesConfig()
            {
                Delay = x.Delay,
                Encoding = x.Encoding,
                FileUrl = x.FileUrl,
                IsSelected = x.IsSelected
            }));

            return config;
        }

        private IFileService fileService;
        public IFileService FileService
        {
            get { return fileService; }
            set { SetProperty(ref fileService, value); }
        }

        private string startupPath;
        public string StartupPath
        {
            get { return startupPath; }
            set { SetProperty(ref startupPath, value); }
        }

        public List<string> Extensions { get; } = new List<string>() { ".srt", ".txt", ".ass" };

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
                    .Select(s => s.Name).ToList();
                uint width = 0;
                uint height = 0;
                VlcPlayerHelper.MediaPlayer.Size(0, ref width, ref height);
                mi.Resolution = width + "x" + height;
                MediaInfo = mi;
            }
            IsControlVisible = false;
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
