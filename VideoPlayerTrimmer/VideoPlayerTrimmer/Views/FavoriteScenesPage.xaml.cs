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
    public class FavoriteScenesContentPage : BaseContentPage<FavoriteScenesViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FavoriteScenesPage : FavoriteScenesContentPage
    {
        public FavoriteScenesPage()
        {
            InitializeComponent();
            BindingContext = App.Container.Resolve<FavoriteScenesViewModel>();
        }
    }
}