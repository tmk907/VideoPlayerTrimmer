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
    public class VideoPlayerViewModel : BaseViewModel, INavigatingAware, IApplicationLifecycleAware
    {
        private readonly INavigationService navigationService;
        private readonly MediaPlayerService playerService;

        private string filePath;

        public VideoPlayerViewModel(INavigationService navigationService, MediaPlayerService playerService)
        {
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

        public override Task InitializeAsync()
        {
            MediaPlayer = playerService.GetMediaPlayer(filePath);
            
            return Task.CompletedTask;
        }

        public override Task UninitializeAsync()
        {
            MediaPlayer.Pause();
            return Task.CompletedTask;
        }

        public DelegateCommand PlayPauseCommand { get; }
        public DelegateCommand FullScreenCommand { get; }

        private bool isPlaying => MediaPlayer.IsPlaying;

        public void TogglePlayPause()
        {
            if (isPlaying) MediaPlayer.Pause();
            else Play();
        }

        private void Play()
        {
            if (IsVideoViewInitialized)
            {
                MediaPlayer.Play();
            }
        }

        public void Pause()
        {
            MediaPlayer.Pause();
        }

        private void EnableFullscreen()
        {
            MediaPlayer.ToggleFullscreen();
        }

        public void OnResume()
        {
        }

        public void OnSleep()
        {
        }
    }
}
