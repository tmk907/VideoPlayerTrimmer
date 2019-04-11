using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class VideosViewModel : BaseViewModel, INavigatingAware
    {
        private readonly INavigationService navigationService;
        private readonly IVideoLibrary videoLibrary;

        public VideosViewModel(INavigationService navigationService, IVideoLibrary videoLibrary)
        {
            this.navigationService = navigationService;
            this.videoLibrary = videoLibrary;
            ItemTappedCommand = new Command<VideoItem>(async (item) => await NavigateToPlayerPage(item));
        }

        public string Directory { get; set; }
        public ObservableRangeCollection<VideoItem> VideoItems { get; set; } = new ObservableRangeCollection<VideoItem>();

        public Command ItemTappedCommand { get; set; }

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

        public async Task NavigateToPlayerPage(VideoItem item)
        {
            var parameter = new NavigationParameters();
            parameter.Add(NavigationParameterNames.VideoPath, item.FilePath);
            await navigationService.NavigateAsync(PageNames.Player, parameter);
        }
    }
}
