using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Prism;
using Prism.DryIoc;
using Prism.Ioc;
using System;
using System.Linq;
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
            await NavigationService.NavigateAsync("NavigationPage/HomePage?selectedTab=folders");

            var a = Container.Resolve<MediaPlayerService>();
        }
        
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<HomePage>();
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
            containerRegistry.RegisterSingleton<MediaPlayerService>();
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
            App.DebugLog("1");
            base.OnSleep();
            App.DebugLog("2");
        }

        protected override void OnResume()
        {
            App.DebugLog("1");
            base.OnResume();
            App.DebugLog("2");
        }

        public static void DebugLog(string arg, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
            var callerTypeName = filePath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
            callerTypeName = callerTypeName.Remove(callerTypeName.Length - 3);
            System.Diagnostics.Debug.WriteLine("LOG: {0}.{1}() {2}", callerTypeName, methodName, arg);
        }
        
        public static void DebugLog(LoggerArgs args, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
            var callerTypeName = Path.GetFileNameWithoutExtension(filePath);
            DebugLog(callerTypeName, methodName, args.ToString());
        }
    }
}
