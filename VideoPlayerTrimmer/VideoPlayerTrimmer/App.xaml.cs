using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.IO;
using System.Threading.Tasks;
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
    public partial class App : PrismApplication
    {
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            VersionTracking.Track();

            await NavigationService.NavigateAsync("NavigationPage/folders");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<FavoriteScenesPage, FavoriteScenesViewModel>(PageNames.Favourites);
            containerRegistry.RegisterForNavigation<FoldersPage, FoldersViewModel>(PageNames.Folders);
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>(PageNames.Settings);
            containerRegistry.RegisterForNavigation<TrimmerPage, TrimmerViewModel>(PageNames.Trimmer);
            containerRegistry.RegisterForNavigation<VideoPlayerPage, VideoPlayerViewModel>(PageNames.Player);
            containerRegistry.RegisterForNavigation<VideosPage, VideosViewModel>(PageNames.Videos);

            RegisterServices(containerRegistry);
        }

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


        private void RegisterServices(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance<VideoDatabase>(Database);
            containerRegistry.RegisterSingleton<IVideoLibrary, VideoLibrary>();

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
