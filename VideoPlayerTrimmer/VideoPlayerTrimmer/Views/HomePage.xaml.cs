using Plugin.Iconize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.Views
{
    [AdMaiora.RealXaml.Client.RootPage]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomePage : IconTabbedPage
    {
        public HomePage()
        {
            AdMaiora.RealXaml.Client.AppManager.Init(this);
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}