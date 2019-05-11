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

        private bool backToTrimmer = false;

        public ObservableRangeCollection<VideoItem> VideoItems { get; set; } = new ObservableRangeCollection<VideoItem>();

        public Command ItemTappedCommand { get; set; }

        public override async Task OnAppearingAsync(bool firstTime)
        {
            if (firstTime)
            {
                await LoadDataAsync();
            }
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            Directory = (string)parameters[NavigationParameterNames.Directory];
            if (parameters.ContainsKey(NavigationParameterNames.GoBack))
            {
                backToTrimmer = parameters.GetValue<bool>(NavigationParameterNames.GoBack);
            }
        }

        private async Task LoadDataAsync()
        {
            var list = await videoLibrary.GetVideoItemsAsync(Directory, false);
            VideoItems.Clear();
            VideoItems.AddRange(list.OrderBy(e => e.Title));
        }

        public async Task NavigateToPlayerPage(VideoItem item)
        {
            var parameters = new NavigationParameters();
            parameters.Add(NavigationParameterNames.VideoPath, item.FilePath);
            if (backToTrimmer)
            {
                await navigationService.GoBackAsync(parameters);
            }
            else
            {
                await navigationService.NavigateAsync(PageNames.Player, parameters);
            }
        }
    }
}
