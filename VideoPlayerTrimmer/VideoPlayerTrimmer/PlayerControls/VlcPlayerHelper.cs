using LibVLCSharp.Forms.Shared;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class VlcPlayerHelper : BindableBase
    {
        private readonly MediaPlayerBuilder _mediaPlayerBuilder;
        private AspectRatio CurrentAspectRatio = AspectRatio.Original;


        public VlcPlayerHelper(MediaPlayerBuilder mediaPlayerBuilder)
        {
            _mediaPlayerBuilder = mediaPlayerBuilder;

            PlayPauseCommand = new Command(() => OnPlayPauseClicked());
            AspectRatioClickedCommand = new Command(() => OnAspectRatioClicked());
            SliderValueChangedCommand = new Command<TimeSpan>((val) => OnSliderValueChanged(val));
        }

        private MediaPlayer mediaPlayer;
        public MediaPlayer MediaPlayer
        {
            get => mediaPlayer;
            private set => SetProperty(ref mediaPlayer, value);
        }

        private LibVLC libVLC;
        public LibVLC LibVLC
        {
            get => libVLC;
            private set => SetProperty(ref libVLC, value);
        }

        private VideoView videoView;
        public VideoView VideoView
        {
            get => videoView;
            private set => SetProperty(ref videoView, value);
        }

        public void LoadFile(string filePath)
        {
            App.DebugLog("");
            MediaPlayer = _mediaPlayerBuilder.GetMediaPlayer(filePath);
            LibVLC = _mediaPlayerBuilder.LibVLC;

            if (VideoView == null)
            {
                VideoView = new VideoView();
            }
            VideoView.MediaPlayer = MediaPlayer;

            InitMediaPlayer();
        }

        private void InitMediaPlayer()
        {
            App.DebugLog("");

            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.Stopped += MediaPlayer_Stopped;
            MediaPlayer.NothingSpecial += MediaPlayer_NothingSpecial;
            MediaPlayer.EndReached += MediaPlayer_EndReached;
            MediaPlayer.EncounteredError += MediaPlayer_EncounteredError;
        }

        private void UninitMediaPlayer()
        {
            App.DebugLog("");

            MediaPlayer.TimeChanged -= MediaPlayer_TimeChanged;
            MediaPlayer.LengthChanged -= MediaPlayer_LengthChanged;
            MediaPlayer.Playing -= MediaPlayer_Playing;
            MediaPlayer.Paused -= MediaPlayer_Paused;
            MediaPlayer.Stopped -= MediaPlayer_Stopped;
            MediaPlayer.NothingSpecial -= MediaPlayer_NothingSpecial;
            MediaPlayer.EndReached -= MediaPlayer_EndReached;
            MediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;
        }

        public void OnAppearing()
        {
            App.DebugLog("");

            VideoView = new VideoView();
            VideoView.MediaPlayer = MediaPlayer;
        }

        public void OnDisappearing()
        {
            App.DebugLog("");

            VideoView = null;
        }


        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {   
            get { return totalTime; }
            set { SetProperty(ref totalTime, value); }
        }


        private TimeSpan elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return elapsedTime; }
            set { SetProperty(ref elapsedTime, value); }
        }


        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        public Command PlayPauseCommand { get; }
        public Command AspectRatioClickedCommand { get; }
        public Command<TimeSpan> SliderValueChangedCommand { get; }

        private void OnPlayPauseClicked()
        {
            switch (mediaPlayer.State)
            {
                case VLCState.Ended:
                    mediaPlayer.Stop();
                    goto case VLCState.Stopped;
                case VLCState.Error:
                case VLCState.Paused:
                case VLCState.Stopped:
                case VLCState.NothingSpecial:
                    mediaPlayer.Play();
                    break;
                default:
                    mediaPlayer.Pause();
                    break;
            }
        }

        private void OnSliderValueChanged(TimeSpan value)
        {
            if (value == null) return;
            if (MediaPlayer.IsSeekable)
            {
                long newTime = (long)value.TotalMilliseconds;
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

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            var state = MediaPlayer?.State;
            var length = MediaPlayer == null ||
                state == VLCState.Ended || state == VLCState.Error || state == VLCState.NothingSpecial ||
                state == VLCState.Stopped ? 0 : MediaPlayer.Length;
            App.DebugLog(TimeSpan.FromMilliseconds(length).ToString());
            Device.BeginInvokeOnMainThread(() => { TotalTime = TimeSpan.FromMilliseconds(length); });
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() => { ElapsedTime = TimeSpan.FromMilliseconds(e.Time); });
            //ShowIsFavorite(currentTime);
        }

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            App.DebugLog("");
            UpdateState(VLCState.Ended);
        }

        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            App.DebugLog("");
            //TODO
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            App.DebugLog("");
            UpdateState(VLCState.Paused);
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            App.DebugLog("");
            UpdateState(VLCState.Playing);
        }

        private void MediaPlayer_NothingSpecial(object sender, EventArgs e)
        {
            App.DebugLog("");
            UpdateState(VLCState.NothingSpecial);
        }

        private void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            App.DebugLog("");
            UpdateState(VLCState.Stopped);
        }

        private void UpdateState(VLCState playbackState)
        {
            if(playbackState == VLCState.Playing)
            {
                Device.BeginInvokeOnMainThread(() => { IsPlaying = true; });
            }
            else if (playbackState == VLCState.Paused)
            {
                Device.BeginInvokeOnMainThread(() => { IsPlaying = false; });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() => { IsPlaying = false; ElapsedTime = TimeSpan.Zero; });
            }
        }

        private void OnAspectRatioClicked()
        {
            if (mediaPlayer == null)
            {
                return;
            }

            try
            {
                var videoView = VideoView;
                if (videoView == null)
                {
                    throw new NullReferenceException($"The {nameof(VideoView)} property must be set in order to use the aspect ratio feature.");
                }

                MediaTrack? mediaTrack;
                try
                {
                    mediaTrack = mediaPlayer.Media?.Tracks?.FirstOrDefault(x => x.TrackType == TrackType.Video);
                }
                catch (Exception)
                {
                    mediaTrack = null;
                }
                if (mediaTrack == null || !mediaTrack.HasValue)
                {
                    return;
                }

                ChangeCurrentAspectRatio();

                var scalingFactor = Device.Info.ScalingFactor;
                var displayW = videoView.Width * scalingFactor;
                var displayH = videoView.Height * scalingFactor;

                switch (CurrentAspectRatio)
                {
                    case AspectRatio.BestFit:
                        mediaPlayer.AspectRatio = string.Empty;
                        mediaPlayer.Scale = 0;
                        break;
                    case AspectRatio.FitScreen:
                    case AspectRatio.Fill:
                        var videoSwapped = mediaTrack.Value.Data.Video.Orientation == VideoOrientation.LeftBottom ||
                            mediaTrack.Value.Data.Video.Orientation == VideoOrientation.RightTop;
                        if (CurrentAspectRatio == AspectRatio.FitScreen)
                        {
                            var videoW = mediaTrack.Value.Data.Video.Width;
                            var videoH = mediaTrack.Value.Data.Video.Height;

                            if (videoSwapped)
                            {
                                var swap = videoW;
                                videoW = videoH;
                                videoH = swap;
                            }
                            if (mediaTrack.Value.Data.Video.SarNum != mediaTrack.Value.Data.Video.SarDen)
                                videoW = videoW * mediaTrack.Value.Data.Video.SarNum / mediaTrack.Value.Data.Video.SarDen;

                            var ar = videoW / (float)videoH;
                            var dar = displayW / (float)displayH;

                            float scale;
                            if (dar >= ar)
                                scale = (float)displayW / videoW; /* horizontal */
                            else
                                scale = (float)displayH / videoH; /* vertical */

                            mediaPlayer.Scale = scale;
                            mediaPlayer.AspectRatio = string.Empty;
                        }
                        else
                        {
                            mediaPlayer.Scale = 0;
                            mediaPlayer.AspectRatio = videoSwapped ? $"{displayH}:{displayW}" : $"{displayW}:{displayH}";
                        }
                        break;
                    case AspectRatio._16_9:
                        mediaPlayer.AspectRatio = "16:9";
                        mediaPlayer.Scale = 0;
                        break;
                    case AspectRatio._4_3:
                        mediaPlayer.AspectRatio = "4:3";
                        mediaPlayer.Scale = 0;
                        break;
                    case AspectRatio.Original:
                        mediaPlayer.AspectRatio = string.Empty;
                        mediaPlayer.Scale = 1;
                        break;
                }

                //AspectRatioLabel.Text = AspectRatioLabels[CurrentAspectRatio];
                //await AspectRatioLabel.FadeTo(1);
                //await AspectRatioLabel.FadeTo(0, 2000);
            }
            catch (Exception ex)
            {
                //TODO
                //ShowErrorMessageBox(ex);
            }
        }

        private void ChangeCurrentAspectRatio()
        {
            var newRatio = AspectRatio.Original;
            switch (CurrentAspectRatio)
            {
                case AspectRatio.BestFit:
                    newRatio = AspectRatio.Fill;
                    break;
                case AspectRatio.Fill:
                    newRatio = AspectRatio.FitScreen;
                    break;
                case AspectRatio.FitScreen:
                    newRatio = AspectRatio.Original;
                    break;
                case AspectRatio.Original:
                    newRatio = AspectRatio.BestFit;
                    break;
                default:
                    newRatio = AspectRatio.Original;
                    break;
            }
            CurrentAspectRatio = newRatio;
        }
    }

    public enum AspectRatio
    {
        BestFit,
        FitScreen,
        Fill,
        _16_9,
        _4_3,
        Original
    }
}
