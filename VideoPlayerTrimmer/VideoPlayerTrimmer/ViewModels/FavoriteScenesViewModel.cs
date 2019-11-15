using AsyncAwaitBestPractices.MVVM;
using System;
using System.Collections;
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
    public class FavoriteScenesViewModel : BaseViewModel
    {
        private readonly IVideoLibrary videoLibrary;

        public FavoriteScenesViewModel(IVideoLibrary videoLibrary)
        {
            this.videoLibrary = videoLibrary;
            ItemTappedCommand = new AsyncCommand<object>((e) => OnItemClicked(e));
        }

        public IAsyncCommand<object> ItemTappedCommand { get; }

        protected override async Task InitializeVMAsync(CancellationToken token)
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
            var scene = item as FavoriteScene;
            var video = GroupedFavoriteScenes.FirstOrDefault(v => v.Favorites.Any(s => s.SnapshotPath == scene.SnapshotPath));
            await App.NavigationService.NavigateToAsync($"{PageNames.Player}" +
                $"?{NavigationParameterNames.VideoPath}={video.VideoPath}" +
                $"&{NavigationParameterNames.Position}={scene.Position}", false);
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
