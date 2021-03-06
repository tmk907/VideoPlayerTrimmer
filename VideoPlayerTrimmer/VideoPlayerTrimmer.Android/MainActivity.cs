﻿using System;

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
using LibVLCSharp.Forms.Shared;
using VideoPlayerTrimmer.Droid.BroadcastReceivers;
using LibVLCSharp.Forms.Platforms.Android;
using VideoPlayerTrimmer.FilePicker;

namespace VideoPlayerTrimmer.Droid
{
    [Activity(Label = "VideoPlayerTrimmer", Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_round_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static string VolumeChangeMessage = "VolumeChangeMessage";

        private VolumeChangeBroadcastReceiver volumeReceiver;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Platform.Init(this);
            InitRenderersAndServices(savedInstanceState);
            //volumeReceiver = new VolumeChangeBroadcastReceiver();
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.SetFlags(new[] { "CollectionView_Experimental", "Shell_Experimental" });
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            RegisterServices();

            Android.Glide.Forms.Init(this);
            LoadApplication(new App());
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

        public void RegisterServices()
        {
            App.DIContainer.Register<IMediaScanner, MediaScannerImpl>();
            App.DIContainer.Register<IVolumeService, VolumeService>().AsSingleton();
            App.DIContainer.Register<IBrightnessService, BrightnessService>();
            App.DIContainer.Register<IOrientationService, OrientationService>();
            App.DIContainer.Register<IStatusBarService, StatusBarService>();
            App.DIContainer.Register<IFFmpegService, FFmpegService2>();
            App.DIContainer.Register<IFFmpegConverter, FFmpegConverter>();
            App.DIContainer.Register<IFileService, FileService>();
        }
    }
}