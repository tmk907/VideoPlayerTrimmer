using LibVLCSharp.Forms.Shared;
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
        private Dictionary<int, string> spuToFileUrlMapping = new Dictionary<int, string>();

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

        public VideoView VideoView { get; set; }
        public event Action<AspectRatio> AspectRatioChanged;

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
            spuToFileUrlMapping.Clear();

            mediaPlayer.Playing += MediaPlayerStartedPlaying;
            MediaPlayer.Play();
        }

        private async void MediaPlayerStartedPlaying(object sender, EventArgs e)
        {
            mediaPlayer.Playing -= MediaPlayerStartedPlaying;
            await Task.Delay(100);
            mediaPlayer.Pause();

            GetVlcSubtitles();
            
            if (_startupConfiguration.IsEmbeddedSubtitlesSelected)
            {
                mediaPlayer.SetSpu(_startupConfiguration.SelectedSubtitlesSpu);
                ChangeSubtitlesDelay(_startupConfiguration.EmbeddedSubtitlesDelay);
            }
            else if (_startupConfiguration.IsFileSubtitlesSelected)
            {
                var selectedFileSpu = spuToFileUrlMapping
                    .FirstOrDefault(x => x.Value == _startupConfiguration.SelectedSubtitlesFile.FileUrl).Key;
                
                if (selectedFileSpu != -1)
                {
                    mediaPlayer.SetSpu(selectedFileSpu);
                    ChangeSubtitlesDelay(_startupConfiguration.EmbeddedSubtitlesDelay);
                }
            }
            
            mediaPlayer.Time = (long)_startupConfiguration.ResumeTime;
            if (_startupConfiguration.AutoPlay)
            {
                MediaPlayer.Play();
            }

            PlayerReady?.Invoke();
        }

        public List<VlcSubtitles> GetVlcSubtitles()
        {
            var list = new List<VlcSubtitles>();

            spuToFileUrlMapping.Clear();

            var allSubs = mediaPlayer.SpuDescription.Where(x => x.Id != -1);
            var extSubs = mediaPlayer.Media.Slaves.Where(x => x.Type == MediaSlaveType.Subtitle);

            var embSubsCount = allSubs.Count() - extSubs.Count();

            if (allSubs.Count() > 0)
            {
                list.Add(new VlcSubtitles(-1, "Disable"));
            }

            foreach (var spu in allSubs.Take(embSubsCount))
            {
                spuToFileUrlMapping.Add(spu.Id, "");
                list.Add(new VlcSubtitles(spu.Id,spu.Name)
                {
                    IsSelected = mediaPlayer.Spu == spu.Id
                });
            }
            int i = 0;
            foreach (var spu in allSubs.Skip(embSubsCount))
            {
                spuToFileUrlMapping[spu.Id] = extSubs.ElementAt(i).Uri;
                list.Add(new VlcSubtitles(spu.Id, spu.Name)
                {
                    IsSelected = mediaPlayer.Spu == spu.Id,
                    FileUrl = extSubs.ElementAt(i).Uri
                });
                i++;
            }
            return list;
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
            if (mediaPlayer == null || VideoView == null)
            {
                App.DebugLog("MediaPlayer or VideoView is null");
                return;
            }

            try
            {
                var videoView = VideoView;
                
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

                switch (_currentAspectRatio)
                {
                    case AspectRatio.BestFit:
                        mediaPlayer.AspectRatio = string.Empty;
                        mediaPlayer.Scale = 0;
                        break;
                    case AspectRatio.FitScreen:
                    case AspectRatio.Fill:
                        var videoSwapped = mediaTrack.Value.Data.Video.Orientation == VideoOrientation.LeftBottom ||
                            mediaTrack.Value.Data.Video.Orientation == VideoOrientation.RightTop;
                        if (_currentAspectRatio == AspectRatio.FitScreen)
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
                AspectRatioChanged?.Invoke(_currentAspectRatio);
            }
            catch (Exception ex)
            {
                App.DebugLog(ex.ToString());
            }
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

        //public IEnumerable<VlcSubtitles> GetSubtitleTracks()
        //{
        //    var subtitles = new List<VlcSubtitles>();
        //    var ids = mediaPlayer.Media.Tracks.Where(x => x.TrackType == TrackType.Text).Select(x => x.Id).ToList();
        //    foreach (var item in mediaPlayer.SpuDescription)
        //    {
        //        subtitles.Add(new VlcSubtitles(item.Id, item.Name));
        //    }            
        //    var sub = subtitles.SingleOrDefault(a => a.Spu == mediaPlayer.Spu);
        //    if (sub != null)
        //    {
        //        sub.IsSelected = true;
        //    }

        //    return subtitles;
        //}

        public void SetSubtitles(VlcSubtitles subtitles)
        {
            mediaPlayer.SetSpu(subtitles.Spu);
            //var fileUrl = mediaPlayer.Spu == -1 ? "" : spuToFileUrlMapping[mediaPlayer.Spu];
            //var prevFileUrl = spuToFileUrlMapping[subtitles.Spu];

            //if (fileUrl == "" && prevFileUrl == "")
            //{

            //}

            //var newSubs = _startupConfiguration.ExternalSubtitles.FirstOrDefault(x => x.FileUrl == fileUrl);
            //if (_startupConfiguration.IsEmbeddedSubtitlesSelected)
            //{
            //    if (newSubs == null || _startupConfiguration.EmbeddedSubtitlesEncoding == newSubs.Encoding)
            //    {
            //        mediaPlayer.SetSpu(subtitles.Spu);
            //        return;
            //    }
            //}
            //else if (_startupConfiguration.IsFileSubtitlesSelected && _startupConfiguration.SelectedSubtitlesFile.Encoding == newSubs.Encoding)
            //{
            //    mediaPlayer.SetSpu(subtitles.Spu);
            //    return;
            //}

            //_startupConfiguration.ResumeTime = mediaPlayer.Time;
            //foreach(var sub in _startupConfiguration.ExternalSubtitles)
            //{
            //    sub.IsSelected = false;
            //}
            //newSubs.IsSelected = true;

            //mediaPlayer.Stop();

            //StartPlayback();
        }

        public void Reset(StartupConfiguration startupConfiguration)
        {
            startupConfiguration.ResumeTime = mediaPlayer.Time;
            _startupConfiguration = startupConfiguration;
            mediaPlayer.Stop();
            StartPlayback();
        }

        public void ChangeSubtitlesDelay(TimeSpan delay)
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
