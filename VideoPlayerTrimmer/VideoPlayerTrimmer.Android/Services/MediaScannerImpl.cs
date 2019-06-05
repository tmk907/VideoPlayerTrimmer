using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Provider;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using VideoPlayerTrimmer.Extensions;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;
using VideoPlayerTrimmer.Models;
using Android.Content;

[assembly: Dependency(typeof(MediaScannerImpl))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class MediaScannerImpl : IMediaScanner
    {
        public async Task<List<VideoSource>> ScanVideosAsync()
        {
            await RequestStoragePermission();
            return GetVideoSources();
        }

        private List<VideoSource> GetVideoSources()
        {
            string[] projection = {
                MediaStore.Video.VideoColumns.Data,
                MediaStore.Video.VideoColumns.DisplayName,
                MediaStore.Video.VideoColumns.DateAdded,
                MediaStore.Video.VideoColumns.DateModified,
                MediaStore.Video.VideoColumns.Duration,
                MediaStore.Video.VideoColumns.Id,
                MediaStore.Video.VideoColumns.Size,
                MediaStore.Video.VideoColumns.Title,
            };
            var cursor = CrossCurrentActivity.Current.Activity.ContentResolver.Query(MediaStore.Video.Media.ExternalContentUri,
                    projection, null, null, MediaStore.Video.VideoColumns.Data);
            var videoSources = new List<VideoSource>();
            try
            {
                cursor.MoveToFirst();
                do
                {
                    var videoSource = new VideoSource();
                    videoSource.FilePath = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.Data));
                    videoSource.FileName = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.DisplayName));
                    var dateAddedTimestamp = cursor.GetLong(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.DateAdded));
                    videoSource.DateAdded = dateAddedTimestamp.DateTimeFromUnixTimestamp();
                    var dateModifiedTimestamp = cursor.GetLong(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.DateModified));
                    videoSource.DateModified = dateModifiedTimestamp.DateTimeFromUnixTimestamp();
                    videoSource.Duration = TimeSpan.FromMilliseconds(
                        cursor.GetLong(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.Duration)));
                    videoSource.MediaStoreId = cursor.GetLong(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.Id));
                    videoSource.SizeInBytes = cursor.GetLong(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.Size));
                    videoSource.Title = cursor.GetString(cursor.GetColumnIndexOrThrow(MediaStore.Video.VideoColumns.Title));
                    videoSources.Add(videoSource);
                } while (cursor.MoveToNext());

                cursor.Close();
            }
            catch (Exception e)
            {
            }
            return videoSources;
        }

        private async Task RequestStoragePermission()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
                        //await DisplayAlert("Need location", "Gunna need that location", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);

                    if (results.ContainsKey(Permission.Storage))
                    {
                        status = results[Permission.Storage];
                    }
                }

                if (status == PermissionStatus.Granted)
                {
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //await DisplayAlert("Location Denied", "Can not continue, try again.", "OK");
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void AddVideo(string path)
        {
            Intent intent = new Intent(Intent.ActionMediaScannerScanFile);
            intent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
            CrossCurrentActivity.Current.AppContext.SendBroadcast(intent);
        }
    }
}