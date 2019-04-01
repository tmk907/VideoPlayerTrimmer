﻿using System;

using Android.App;
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

namespace VideoPlayerTrimmer.Droid
{
    [Activity(Label = "VideoPlayerTrimmer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            InitRenderersAndServices(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.SetFlags(new[] { "CollectionView_Experimental", "Shell_Experimental" });
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            RegisterPlatformServices();

            LoadApplication(new App());
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

        private void RegisterPlatformServices()
        {
            DependencyService.Register<IMediaScanner, MediaScannerImpl>();
        }
    }
}