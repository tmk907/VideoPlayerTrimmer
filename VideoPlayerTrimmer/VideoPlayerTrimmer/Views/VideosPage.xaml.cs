using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    public class VideosContentPage : BaseContentPage<VideosViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VideosPage : VideosContentPage
    {
        public VideosPage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
        }
    }
}