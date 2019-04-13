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

        private string filePath;

        public VideoPlayerViewModel(INavigationService navigationService, MediaPlayerService playerService)
        {
            App.DebugLog("");
            this.navigationService = navigationService;
            this.playerService = playerService;
            PlayPauseCommand = new DelegateCommand(() => TogglePlayPause());
            FullScreenCommand = new DelegateCommand(() => EnableFullscreen());
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

        public override Task InitializeAsync()
        {
            App.DebugLog("");
            InitMediaPlayer();
            return Task.CompletedTask;
        }
        
        private void InitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer = playerService.GetMediaPlayer(filePath);
        }

        private void UnInitMediaPlayer()
        {
            App.DebugLog("");
            MediaPlayer.Pause();
            position = MediaPlayer.Position;
            MediaPlayer.Stop();
        }

        public DelegateCommand PlayPauseCommand { get; }
        public DelegateCommand FullScreenCommand { get; }

        private float position = 0;
        private bool isPlaying => MediaPlayer.IsPlaying;
        private bool isPausedByUser = false;

        public void TogglePlayPause()
        {
            if (isPlaying)
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
            MediaPlayer.Position = position;
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

        private void EnableFullscreen()
        {
            
        }

    }
}
