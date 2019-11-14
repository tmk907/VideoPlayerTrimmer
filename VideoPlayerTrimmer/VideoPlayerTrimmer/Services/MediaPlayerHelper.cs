using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;

namespace VideoPlayerTrimmer.Services
{
    public class MediaPlayerHelper : BindableBase
    {
        private readonly MediaPlayerBuilder playerService;
        public MediaPlayerHelper(MediaPlayerBuilder playerService)
        {
            this.playerService = playerService;
        }

        public event EventHandler MediaPlayerReady;
        public event EventHandler<PlaybackStateEventArgs> PlaybackStateChanged;

        public event EventHandler<MediaPlayerTimeChangedEventArgs> TimeChanged
        {
            add
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.TimeChanged += value;
                }
            }
            remove
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.TimeChanged -= value;
                }
            }
        }

        private void OnMediaPlayerReady()
        {
            App.DebugLog("");
            MediaPlayerReady?.Invoke(this, new EventArgs());
        }

        private void OnPlaybackStateChanged(PlaybackState state)
        {
            App.DebugLog(state.ToString());
            PlaybackStateChanged?.Invoke(this, new PlaybackStateEventArgs(state));
        }

        private MediaPlayer mediaPlayer;
        public MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
            set { SetProperty(ref mediaPlayer, value); }
        }

        public void InitMediaPlayer(string filePath)
        {
            App.DebugLog("");
            MediaPlayer = playerService.GetMediaPlayer(filePath);

            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
            MediaPlayer.SnapshotTaken += MediaPlayer_SnapshotTaken;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.Playing += MediaPlayer_Playing;
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            App.DebugLog("");
            OnPlaybackStateChanged(PlaybackState.Playing);
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            App.DebugLog("");
            OnPlaybackStateChanged(PlaybackState.Paused);
        }

        public void UnInitMediaPlayer()
        {
            App.DebugLog("");
            if (!IsInitialized) return;
            MediaPlayer.Pause();
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.Playing -= MediaPlayer_Playing;
            MediaPlayer.EndReached -= MediaPlayer_EndReached;
            MediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
            MediaPlayer.SnapshotTaken -= MediaPlayer_SnapshotTaken;
            MediaPlayer.Stop();
            MediaPlayer.Dispose();
            MediaPlayer = null;
            OnPlaybackStateChanged(PlaybackState.Stopped);
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

        public bool IsInitialized { get => mediaPlayer != null; }
        public bool IsVideoViewInitialized { get; private set; }

        public bool IsPausedByUser { get; set; } = false;

        public bool IsPlaying { get => MediaPlayer.IsPlaying; }

        public void TogglePlayPause()
        {
            App.DebugLog("");
            if (MediaPlayer.IsPlaying)
            {
                Pause();
                IsPausedByUser = true;
            }
            else
            {
                Play();
                IsPausedByUser = false;
            }
        }

        public void Play()
        {
            App.DebugLog("");
            if (!IsVideoViewInitialized)
            {
                App.DebugLog("VideoView not initialized");
                return;
            }
            MediaPlayer.Play();
        }

        public void Pause()
        {
            App.DebugLog("");
            MediaPlayer.Pause();
        }

        public void StartPlayingOrResume()
        {
            App.DebugLog("");
            IsVideoViewInitialized = true;
            MediaPlayer.Playing += MediaPlayer_PlayingOnInit;
            MediaPlayer.Play();
        }

        public void SeekTo(TimeSpan position)
        {
            if (MediaPlayer!=null && MediaPlayer.IsSeekable)
            {
                long newTime = (long)position.TotalMilliseconds;
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

        private async void MediaPlayer_PlayingOnInit(object sender, EventArgs e)
        {
            App.DebugLog("");
            if (IsPausedByUser)
            {
                // If Pause() is called without delay, videoview is black
                MediaPlayer.Mute = true;
                await Task.Delay(100);
                MediaPlayer.Pause();
                MediaPlayer.Mute = false;
            }
            MediaPlayer.Playing -= MediaPlayer_PlayingOnInit;
            OnMediaPlayerReady();
        }
    }
}
