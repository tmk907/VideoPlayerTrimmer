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
        public VideoPlayerPage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private void MediaPlayerChanged(object sender, System.EventArgs e)
        {
            ViewModel.IsVideoViewInitialized = true;
            ViewModel.StartPlayingOrResume();
        }
    }
}