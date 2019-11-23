using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.MediaHelpers;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.PlayerControls
{
    public class VlcPlayerHelper : BindableBase
    {
        private readonly MediaPlayerBuilder _mediaPlayerBuilder;
        private AspectRatio _currentAspectRatio = AspectRatio.Original;
        private StartupConfiguration _startupConfiguration;

        public event Action PlayerReady;

        public VlcPlayerHelper(MediaPlayerBuilder mediaPlayerBuilder)
        {
            _mediaPlayerBuilder = mediaPlayerBuilder;

            PlayPauseCommand = new Command(() => OnPlayPauseClicked());
            AspectRatioClickedCommand = new Command(() => OnAspectRatioClicked());
            SliderValueChangedCommand = new Command<TimeSpan>((val) => SeekTo(val));
        }

        public Command PlayPauseCommand { get; }
        public Command AspectRatioClickedCommand { get; }
        public Command<TimeSpan> SliderValueChangedCommand { get; }

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


        public void LoadFile(StartupConfiguration startupConfiguration)
        {
            App.DebugLog(startupConfiguration.FilePath);

            _startupConfiguration = startupConfiguration;

            LibVLC = _mediaPlayerBuilder.LibVLC;

            MediaPlayer = _mediaPlayerBuilder.GetMediaPlayer();
            AddMediaPlayerEvents();
            StartPlayback();

            App.DebugLog("");
        }

        private void StartPlayback()
        {
            App.DebugLog("");

            var media = _mediaPlayerBuilder.GetMedia(_startupConfiguration);
            MediaPlayer.Media = media;

            mediaPlayer.Playing += MediaPlayerStartedPlaying;
            MediaPlayer.Play();
        }

        private async void MediaPlayerStartedPlaying(object sender, EventArgs e)
        {
            mediaPlayer.Playing -= MediaPlayerStartedPlaying;
            await Task.Delay(100);
            mediaPlayer.Pause();

            if (_startupConfiguration.IsEmbeddedSubtitlesSelected)
            {
                mediaPlayer.SetSpu(_startupConfiguration.SelectedSubtitlesSpu);
                SetSubtitlesDelay(_startupConfiguration.EmbeddedSubtitlesDelay);
            }
            else if (_startupConfiguration.IsFileSubtitlesSelected)
            {
                var spuIds = mediaPlayer.SpuDescription.Where(x => x.Id != -1).Select(x => x.Id);
                var extSubsCount = mediaPlayer.Media.Slaves.Where(x => x.Type == MediaSlaveType.Subtitle).Count();

                if (spuIds.Count() > 0 && extSubsCount > 0)
                {
                    var embSubsCount = spuIds.Count() - extSubsCount;

                    var slaveIndex = 0;
                    foreach (var item in mediaPlayer.Media.Slaves.Where(x => x.Type == MediaSlaveType.Subtitle))
                    {
                        if (_startupConfiguration.SelectedSubtitlesFileUrl == item.Uri) break;
                        slaveIndex++;
                    }

                    var selectedFileSpu = spuIds.ElementAt(embSubsCount + slaveIndex);

                    mediaPlayer.SetSpu(selectedFileSpu);
                    SetSubtitlesDelay(_startupConfiguration.EmbeddedSubtitlesDelay);
                }
            }
            
            mediaPlayer.Time = (long)_startupConfiguration.ResumeTime;
            if (_startupConfiguration.AutoPlay)
            {
                MediaPlayer.Play();
            }

            PlayerReady?.Invoke();
        }

        public void OnDisappearing()
        {
            App.DebugLog("1");
            RemoveMediaPlayerEvents();
            mediaPlayer.Stop();
            App.DebugLog("2");
        }

        private void AddMediaPlayerEvents()
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
            MediaPlayer.MediaChanged += MediaPlayer_MediaChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            mediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
        }
       
        private void RemoveMediaPlayerEvents()
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
            MediaPlayer.MediaChanged -= MediaPlayer_MediaChanged;
            MediaPlayer.ESAdded -= MediaPlayer_ESAdded;
            mediaPlayer.ESDeleted -= MediaPlayer_ESDeleted;
        }

        #region MediaPlayer events

        private void MediaPlayer_Opening(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_LengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
        {
            App.DebugLog("");

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

        private void MediaPlayer_MediaChanged(object sender, MediaPlayerMediaChangedEventArgs e)
        {
            App.DebugLog("");
        }

        private void MediaPlayer_TracksChanged(object sender, EventArgs e)
        {
            App.DebugLog("");
        }

        #endregion

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

            //try
            //{
            //    var videoView = VideoView;
            //    if (videoView == null)
            //    {
            //        throw new NullReferenceException($"The {nameof(VideoView)} property must be set in order to use the aspect ratio feature.");
            //    }

            //    MediaTrack? mediaTrack;
            //    try
            //    {
            //        mediaTrack = mediaPlayer.Media?.Tracks?.FirstOrDefault(x => x.TrackType == TrackType.Video);
            //    }
            //    catch (Exception)
            //    {
            //        mediaTrack = null;
            //    }
            //    if (mediaTrack == null || !mediaTrack.HasValue)
            //    {
            //        return;
            //    }

            //    ChangeCurrentAspectRatio();

            //    var scalingFactor = Device.Info.ScalingFactor;
            //    var displayW = videoView.Width * scalingFactor;
            //    var displayH = videoView.Height * scalingFactor;

            //    switch (CurrentAspectRatio)
            //    {
            //        case AspectRatio.BestFit:
            //            mediaPlayer.AspectRatio = string.Empty;
            //            mediaPlayer.Scale = 0;
            //            break;
            //        case AspectRatio.FitScreen:
            //        case AspectRatio.Fill:
            //            var videoSwapped = mediaTrack.Value.Data.Video.Orientation == VideoOrientation.LeftBottom ||
            //                mediaTrack.Value.Data.Video.Orientation == VideoOrientation.RightTop;
            //            if (CurrentAspectRatio == AspectRatio.FitScreen)
            //            {
            //                var videoW = mediaTrack.Value.Data.Video.Width;
            //                var videoH = mediaTrack.Value.Data.Video.Height;

            //                if (videoSwapped)
            //                {
            //                    var swap = videoW;
            //                    videoW = videoH;
            //                    videoH = swap;
            //                }
            //                if (mediaTrack.Value.Data.Video.SarNum != mediaTrack.Value.Data.Video.SarDen)
            //                    videoW = videoW * mediaTrack.Value.Data.Video.SarNum / mediaTrack.Value.Data.Video.SarDen;

            //                var ar = videoW / (float)videoH;
            //                var dar = displayW / (float)displayH;

            //                float scale;
            //                if (dar >= ar)
            //                    scale = (float)displayW / videoW; /* horizontal */
            //                else
            //                    scale = (float)displayH / videoH; /* vertical */

            //                mediaPlayer.Scale = scale;
            //                mediaPlayer.AspectRatio = string.Empty;
            //            }
            //            else
            //            {
            //                mediaPlayer.Scale = 0;
            //                mediaPlayer.AspectRatio = videoSwapped ? $"{displayH}:{displayW}" : $"{displayW}:{displayH}";
            //            }
            //            break;
            //        case AspectRatio._16_9:
            //            mediaPlayer.AspectRatio = "16:9";
            //            mediaPlayer.Scale = 0;
            //            break;
            //        case AspectRatio._4_3:
            //            mediaPlayer.AspectRatio = "4:3";
            //            mediaPlayer.Scale = 0;
            //            break;
            //        case AspectRatio.Original:
            //            mediaPlayer.AspectRatio = string.Empty;
            //            mediaPlayer.Scale = 1;
            //            break;
            //    }

            //    //AspectRatioLabel.Text = AspectRatioLabels[CurrentAspectRatio];
            //    //await AspectRatioLabel.FadeTo(1);
            //    //await AspectRatioLabel.FadeTo(0, 2000);
            //}
            //catch (Exception ex)
            //{
            //    //TODO
            //    //ShowErrorMessageBox(ex);
            //}
        }

        private void ChangeCurrentAspectRatio()
        {
            var newRatio = AspectRatio.Original;
            switch (_currentAspectRatio)
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
            _currentAspectRatio = newRatio;
        }

        public IEnumerable<AudioTrackInfo> GetAudioTracks()
        {
            var audioTracks = new List<AudioTrackInfo>();

            foreach (var item in mediaPlayer.AudioTrackDescription)
            {
                audioTracks.Add(new AudioTrackInfo() { Id = item.Id, Name = item.Name });
            }
            audioTracks.SingleOrDefault(a => a.Id == mediaPlayer.AudioTrack).IsSelected = true;

            return audioTracks;
        }

        public void SetAudioTrack(int trackIndex)
        {
            mediaPlayer.SetAudioTrack(trackIndex);
        }

        public void SeekTo(TimeSpan position)
        {
            if (position == null || mediaPlayer == null) return;
            if (MediaPlayer.IsSeekable)
            {
                long newTime = (long)position.TotalMilliseconds;
                if (newTime > MediaPlayer.Length)
                {
                    newTime = newTime - 100;
                }
                MediaPlayer.Time = newTime;
                if (!mediaPlayer.IsPlaying)
                {
                    Device.BeginInvokeOnMainThread(() => { ElapsedTime = TimeSpan.FromMilliseconds(newTime); });
                }
            }
        }

        public IEnumerable<VlcSubtitles> GetSubtitleTracks()
        {
            var subtitles = new List<VlcSubtitles>();
            var ids = mediaPlayer.Media.Tracks.Where(x => x.TrackType == TrackType.Text).Select(x => x.Id).ToList();
            foreach (var item in mediaPlayer.SpuDescription)
            {
                subtitles.Add(new VlcSubtitles(item.Id, item.Name));
            }            
            var sub = subtitles.SingleOrDefault(a => a.Spu == mediaPlayer.Spu);
            if (sub != null)
            {
                sub.IsSelected = true;
            }

            return subtitles;
        }

        public void SetSubtitles(VlcSubtitles subtitles)
        {
            mediaPlayer.SetSpu(subtitles.Spu);
        }

        public void AddSubtitles(string fileUrl)
        {
            _startupConfiguration.ResumeTime = mediaPlayer.Time;
            mediaPlayer.Stop();
            if (!_startupConfiguration.ExternalSubtitles.ContainsKey(fileUrl))
            {
                _startupConfiguration.ExternalSubtitles.Add(fileUrl, TimeSpan.Zero);
            }
            _startupConfiguration.SelectedSubtitlesFileUrl = fileUrl;

            StartPlayback();
        }

        public void SetSubtitlesDelay(TimeSpan delay)
        {
            mediaPlayer.SetSpuDelay((long)delay.TotalMilliseconds);
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
