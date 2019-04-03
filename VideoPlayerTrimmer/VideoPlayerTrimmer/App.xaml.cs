using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.IO;
using System.Threading.Tasks;
using TinyIoC;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Services;
using VideoPlayerTrimmer.ViewModels;
using VideoPlayerTrimmer.Views;
using Xamarin.Essentials;
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
            VersionTracking.Track();
            RegisterRoutes();
            RegisterServices();
            MainPage = new AppShell();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(PageNames.Folders, typeof(FoldersPage));
            Routing.RegisterRoute(PageNames.Videos, typeof(VideosPage));
            Routing.RegisterRoute(PageNames.Player, typeof(VideoPlayerPage));
            Routing.RegisterRoute(PageNames.Favourites, typeof(FavoriteScenesPage));
            Routing.RegisterRoute(PageNames.Trimmer, typeof(TrimmerPage));
            Routing.RegisterRoute(PageNames.Settings, typeof(SettingsPage));
        }

        public static AppShell Shell => Current.MainPage as AppShell;

        private static VideoDatabase database;
        public static VideoDatabase Database
        {
            get
            {
                if (database == null)
                {
                    var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "database.db3");
                    var dbSetup = new DatabaseSetup(dbPath);
                    dbSetup.PerformMigrations();
                    database = new VideoDatabase(dbPath);
                }
                return database;
            }
        }

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
            Container.Register<VideoDatabase>(Database);
            Container.Register<IVideoLibrary, VideoLibrary>().AsSingleton();

            Container.Register<FavoriteScenesViewModel>();
            Container.Register<FoldersViewModel>();
            Container.Register<SettingsViewModel>();
            Container.Register<TrimmerViewModel>();
            Container.Register<VideoPlayerPage>();
            Container.Register<VideosViewModel>();
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

        public static TinyIoCContainer Container => TinyIoCContainer.Current;
    }
}
