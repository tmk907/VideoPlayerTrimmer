using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    public class FoldersContentPage : BaseContentPage<FoldersViewModel> { }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FoldersPage : FoldersContentPage
    {
        public FoldersPage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
        }
    }
}