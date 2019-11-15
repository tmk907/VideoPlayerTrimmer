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
            ItemTappedCommand = new Command<VideoItem>(async (item) => await NavigateToPlayerPage(item));
        }

        public string Directory { get; set; }

        private bool backToTrimmer = false;

        public ObservableRangeCollection<VideoItem> VideoItems { get; set; } = new ObservableRangeCollection<VideoItem>();

        public Command ItemTappedCommand { get; set; }

        public override void OnNavigating(Dictionary<string, string> navigationArgs)
        {
            base.OnNavigating(navigationArgs);

            Directory = navigationParameters[NavigationParameterNames.Directory];
            if (navigationParameters.ContainsKey(NavigationParameterNames.GoBack))
            {
                backToTrimmer = navigationParameters[NavigationParameterNames.GoBack] == "true";
            }
        }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog("");

            var list = await videoLibrary.GetVideoItemsAsync(Directory, false);
            VideoItems.Clear();
            VideoItems.AddRange(list.OrderBy(e => e.Title));
        }

        public async Task NavigateToPlayerPage(VideoItem item)
        {
            if (backToTrimmer)
            {
                await App.NavigationService.NavigateToAsync($"//{PageNames.Trimmer}" +
                    $"?{NavigationParameterNames.VideoPath}={item.FilePath}");
            }
            else
            {
                await App.NavigationService.NavigateToAsync($"{PageNames.Player}" +
                    $"?{NavigationParameterNames.VideoPath}={item.FilePath}");
            }
        }
    }
}
