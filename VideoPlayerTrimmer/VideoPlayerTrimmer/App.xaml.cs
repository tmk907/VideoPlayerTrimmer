using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using System.IO;
using VideoPlayerTrimmer.Database;
using VideoPlayerTrimmer.Services;
using VideoPlayerTrimmer.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using AsyncAwaitBestPractices;
using TinyIoC;
using VideoPlayerTrimmer.Framework;
using System.Reflection;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace VideoPlayerTrimmer
{
    public partial class App : Application
    {
        public static TinyIoCContainer DIContainer => TinyIoCContainer.Current;
        public static NavigationService NavigationService { get; private set; }

        public App()
        {
            InitializeComponent();

            ApplicationSetup();
            RegisterServices();

            MainPage = new AppShell();
            NavigationService = new NavigationService();
        }

        private void ApplicationSetup()
        {

#if DEBUG
            SafeFireAndForgetExtensions.Initialize(true);
#endif
            VersionTracking.Track();

            if (VersionTracking.IsFirstLaunchEver)
            {

            }

            var directoryPath = Path.Combine(FileSystem.AppDataDirectory, Configuration.FavoriteThumbnailsFolderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            directoryPath = Path.Combine(FileSystem.AppDataDirectory, Configuration.SnaphotsFolderName);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void RegisterServices()
        {
            DIContainer.Register<VideoDatabase>(Database);
            DIContainer.Register<IVideoLibrary, VideoLibrary>().AsSingleton();
            DIContainer.Register<MediaPlayerBuilder>().AsSingleton();
            DIContainer.Register<ConverterHelper>();

            //var assembly = typeof(App).GetTypeInfo().Assembly;
            //var vms = AssemblyExtensions.SafeGetTypes(assembly).Where(x => x.IsSubclassOf(typeof(BaseViewModel)));
            //foreach (var vm in vms)
            //{
            //    DIContainer.Register(vm);
            //}

            DIContainer.Register<ViewModels.FavoriteScenesViewModel>();
            DIContainer.Register<ViewModels.FoldersViewModel>();
            DIContainer.Register<ViewModels.SettingsViewModel>();
            DIContainer.Register<ViewModels.TrimmerViewModel>();
            DIContainer.Register<ViewModels.VideoPlayerViewModel>().AsMultiInstance();
            DIContainer.Register<ViewModels.VideosViewModel>();
            //DIContainer.Register<ViewModels.TestViewModel>();

            Routing.RegisterRoute(PageNames.Videos, typeof(VideosPage));
            Routing.RegisterRoute(PageNames.Player, typeof(VideoPlayerPage));

            //containerRegistry.RegisterForNavigation<NavigationPage>();
            //DIContainer.RegisterForNavigation(typeof(HomePage), "HomePage");
            //containerRegistry.RegisterForNavigation<FavoriteScenesPage, FavoriteScenesViewModel>(PageNames.Favourites);
            //containerRegistry.RegisterForNavigation<FoldersPage, FoldersViewModel>(PageNames.Folders);
            //containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>(PageNames.Settings);
            //containerRegistry.RegisterForNavigation<TrimmerPage, TrimmerViewModel>(PageNames.Trimmer);
            //containerRegistry.RegisterForNavigation<VideoPlayerPage>(PageNames.Player);
            //containerRegistry.RegisterForNavigation<VideosPage, VideosViewModel>(PageNames.Videos);
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

        protected override void OnStart()
        {
            // Handle when your app starts
#if DEBUG
            AppCenter.Start($"android={Helpers.Secrets.AppCenter_Android_Secret};", typeof(Crashes));
#else
            AppCenter.Start($"android={Helpers.Secrets.AppCenter_Android_Secret};", typeof(Crashes));
#endif
        }

        public static event Action OnSuspended;
        public static event Action OnResumed;

        protected override void OnSleep()
        {
            App.DebugLog("1");
            OnSuspended?.Invoke();
            App.DebugLog("2");
        }

        protected override void OnResume()
        {
            App.DebugLog("1");
            OnResumed?.Invoke();
            App.DebugLog("2");
        }

        public static void DebugLog(string arg, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
            var callerTypeName = filePath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last();
            callerTypeName = callerTypeName.Remove(callerTypeName.Length - 3);
            var c2 = Path.GetFileNameWithoutExtension(filePath);
            System.Diagnostics.Debug.WriteLine("LOG: {0}.{1}() {2}", callerTypeName, methodName, arg);
        }
        
        public static void DebugLog(LoggerArgs args, [System.Runtime.CompilerServices.CallerFilePath] string filePath = "", [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
        {
            var callerTypeName = Path.GetFileNameWithoutExtension(filePath);
            DebugLog(callerTypeName, methodName, args.ToString());
        }
    }
}
