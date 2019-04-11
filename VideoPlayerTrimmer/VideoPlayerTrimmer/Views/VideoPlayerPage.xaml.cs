using LibVLCSharp.Forms.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    public class VideoPlayerContentPage : BaseContentPage<VideoPlayerViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideoPlayerPage : VideoPlayerContentPage
    {
        VideoView _videoView;
        float _position;


        public VideoPlayerPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<string>(this, "OnPause", app =>
            {
                //VideoView.MediaPlayerChanged -= MediaPlayerChanged;
                ViewModel.Pause();
                _position = ViewModel.MediaPlayer.Position;
                //ViewModel.MediaPlayer.Stop();
                //VideoPlayerGrid.Children.Clear();
                Debug.WriteLine($"saving mediaplayer position {_position}");
            });

            MessagingCenter.Subscribe<string>(this, "OnRestart", app =>
            {
                //_videoView = new VideoView { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
                //VideoPlayerGrid.Children.Add(_videoView);

                //_videoView.MediaPlayerChanged += MediaPlayerChanged;

                //_videoView.MediaPlayer = ViewModel.MediaPlayer;
                //_videoView.MediaPlayer.Position = _position;
                //_position = 0;
                ViewModel.TogglePlayPause();
            });

        }

        private void MediaPlayerChanged(object sender, System.EventArgs e)
        {
            ViewModel.IsVideoViewInitialized = true;
        }

    }
}