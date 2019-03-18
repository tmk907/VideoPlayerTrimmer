using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace VideoPlayerTrimmer
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            RegisterRoutes();
            RegisterServices();
            MainPage = new AppShell();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(PageNames.Folders, typeof(FoldersPage));
            Routing.RegisterRoute(PageNames.Videos, typeof(VideosPage));
            Routing.RegisterRoute(PageNames.Player, typeof(VideoPlayerPage));
            Routing.RegisterRoute(PageNames.Favourites, typeof(FavouriteScenesPage));
            Routing.RegisterRoute(PageNames.Trimmer, typeof(TrimmerPage));
            Routing.RegisterRoute(PageNames.Settings, typeof(SettingsPage));
        }

        public static AppShell Shell => Current.MainPage as AppShell;

        public static Task GoToAsync(string pageName)
        {
            return Shell.GoToAsync($"{RouteScheme}:///{NavRoute}/{pageName}");
        }

        public static Task GoToAsync(string pageName, string query)
        {
            return Shell.GoToAsync($"{RouteScheme}:///{NavRoute}/{pageName}?{query}");
        }

        public const string NavRoute = "videoplayertrimmer";
        public const string RouteScheme = "app";
        public const string RouteHost = "tmk907";

        private void RegisterServices()
        {
            //DependencyService.Register
        }

        protected override void OnStart()
        {
            // Handle when your app starts
#if DEBUG
            AppCenter.Start($"android={Helpers.Secrets.AppCenter_Android_Secret};", typeof(Crashes));
#else
            AppCenter.Start($"android={Helpers.Secrets.AppCenter_Android_Secret};", typeof(Analytics), typeof(Crashes));
#endif
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
