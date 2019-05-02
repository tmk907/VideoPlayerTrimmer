using System;

using Android.App;
using Android.Media;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Xamarin.Forms;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Threading.Tasks;
using Android.Content.PM;
using VideoPlayerTrimmer.Services;
using VideoPlayerTrimmer.Droid.Services;
using Prism;
using Prism.Ioc;
using LibVLCSharp.Forms.Shared;
using VideoPlayerTrimmer.Droid.BroadcastReceivers;

namespace VideoPlayerTrimmer.Droid
{
    [Activity(Label = "VideoPlayerTrimmer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static string VolumeChangeMessage = "VolumeChangeMessage";

        private VolumeChangeBroadcastReceiver volumeReceiver;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            LibVLCSharpFormsRenderer.Init();
            InitRenderersAndServices(savedInstanceState);
            //volumeReceiver = new VolumeChangeBroadcastReceiver();
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.SetFlags(new[] { "CollectionView_Experimental", "Shell_Experimental" });
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Plugin.Iconize.Iconize.Init(Resource.Id.toolbar, Resource.Id.sliding_tabs);

            LoadApplication(new App(new AndroidInitializer()));
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.VolumeUp || keyCode == Keycode.VolumeDown)
            {
                App.DebugLog("KEY volume");
                MessagingCenter.Send(this, VolumeChangeMessage);
            }
            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnResume()
        {
            base.OnResume();
            this.VolumeControlStream = Stream.Music;
            //RegisterReceiver(volumeReceiver, new Android.Content.IntentFilter("android.media.VOLUME_CHANGED_ACTION"));
        }

        protected override void OnPause()
        {
            //UnregisterReceiver(volumeReceiver);
            base.OnPause();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void InitRenderersAndServices(Bundle savedInstanceState)
        {
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IMediaScanner, MediaScannerImpl>();
            containerRegistry.RegisterSingleton<IVolumeService, VolumeService>();
            containerRegistry.Register<IBrightnessService, BrightnessService>();
            containerRegistry.Register<IOrientationService, OrientationService>();
        }
    }
}