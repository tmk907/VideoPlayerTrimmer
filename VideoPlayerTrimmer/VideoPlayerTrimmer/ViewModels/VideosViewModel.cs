using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideosViewModel : BaseViewModel
    {
        private readonly IVideoLibrary videoLibrary;

        public VideosViewModel(IVideoLibrary videoLibrary)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            ItemTappedCommand = new AsyncCommand<object>((item) => NavigateToPlayerPage(item));
        }

        public string Directory { get; set; }

        private bool backToTrimmer = false;

        public ObservableRangeCollection<VideoItem> VideoItems { get; set; } = new ObservableRangeCollection<VideoItem>();

        public IAsyncCommand<object> ItemTappedCommand { get; set; }

        public override void OnNavigating(Dictionary<string, string> navigationArgs)
        {
            base.OnNavigating(navigationArgs);

            if (navigationParameters.ContainsKey(NavigationParameterNames.Directory))
            {
                Directory = navigationParameters[NavigationParameterNames.Directory];
            }
            if (navigationParameters.ContainsKey(NavigationParameterNames.GoBack))
            {
                backToTrimmer = navigationParameters[NavigationParameterNames.GoBack] == true.ToString();
            }
        }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog("");

            var list = await videoLibrary.GetVideoItemsAsync(Directory, false);
            VideoItems.Clear();
            if (backToTrimmer)
            {
                VideoItems.AddRange(list.OrderBy(e => e.Directory).ThenBy(e => e.FileName));
            }
            else
            {
                VideoItems.AddRange(list.OrderBy(e => e.Title));
            }
        }

        public async Task NavigateToPlayerPage(object item)
        {
            var videoItem = item as VideoItem;
            if (backToTrimmer)
            {
                App.NavigationService.BackNavigationParameters = $"{NavigationParameterNames.VideoPath}={videoItem.FilePath}";
                await App.NavigationService.NavigateBackAsync();
                //await App.NavigationService.NavigateToAsync($"{PageNames.TrimmerNav}" +
                //    $"?{NavigationParameterNames.VideoPath}={videoItem.FilePath}");
            }
            else
            {
                await App.NavigationService.NavigateToAsync($"{PageNames.Player}" +
                    $"?{NavigationParameterNames.VideoPath}={videoItem.FilePath}");
            }
        }
    }
}
