using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class FavoriteScenesViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IVideoLibrary videoLibrary;

        public FavoriteScenesViewModel(INavigationService navigationService, IVideoLibrary videoLibrary)
        {
            this.navigationService = navigationService;
            this.videoLibrary = videoLibrary;
            ItemTappedCommand = new DelegateCommand<object>((e) => OnItemClicked(e));
        }

        public DelegateCommand<object> ItemTappedCommand { get; }

        public override async Task OnAppearingAsync(bool firstTime)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            GroupedFavoriteScenes.Clear();
            var videos = await videoLibrary.GetVideoItemsAsync("", false);
            foreach(var video in videos)
            {
                var favScenes = await videoLibrary.GetFavoriteScenes(video.VideoId);
                var favList = new FavoriteList()
                {
                    VideoTitle = video.Title,
                    VideoPath = video.FilePath,
                };
                favList.AddRange(favScenes);
                if (favList.Count > 0)
                {
                    GroupedFavoriteScenes.Add(favList);
                }
            }
        }

        private async Task OnItemClicked(object item)
        {
            var scene = (FavoriteScene)item;
            var parameters = new NavigationParameters();
            var video = GroupedFavoriteScenes.FirstOrDefault(v => v.Favorites.Any(s => s.SnapshotPath == scene.SnapshotPath));
            parameters.Add(NavigationParameterNames.VideoPath, video.VideoPath);
            parameters.Add(NavigationParameterNames.Position, scene.Position);
            await navigationService.NavigateAsync(PageNames.Player, parameters, false);
        }

        public ObservableCollection<FavoriteList> GroupedFavoriteScenes { get; } = new ObservableCollection<FavoriteList>();
    }

    public class FavoriteList : List<FavoriteScene>
    {
        public string VideoTitle { get; set; }

        public string VideoPath { get; set; }

        public List<FavoriteScene> Favorites => this;
    }
}
