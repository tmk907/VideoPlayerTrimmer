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
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}