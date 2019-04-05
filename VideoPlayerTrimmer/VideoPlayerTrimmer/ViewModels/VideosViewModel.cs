using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideosViewModel : BaseViewModel, INavigatingAware
    {
        private readonly IVideoLibrary videoLibrary;

        public VideosViewModel(IVideoLibrary videoLibrary)
        {
            this.videoLibrary = videoLibrary;
        }

        public string Directory { get; set; }
        public ObservableRangeCollection<VideoItem> VideoItems { get; set; } = new ObservableRangeCollection<VideoItem>();

        public override async Task InitializeAsync()
        {
            await LoadDataAsync();
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            Directory = (string)parameters[NavigationParameterNames.Directory];
        }

        private async Task LoadDataAsync()
        {
            var list = await videoLibrary.GetVideoItemsAsync(Directory, false);
            VideoItems.Clear();
            VideoItems.AddRange(list.OrderBy(e => e.Title));
        }

    }
}
