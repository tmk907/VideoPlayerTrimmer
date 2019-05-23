using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class FavoriteScenesViewModel : BaseViewModel
    {
        private readonly IVideoLibrary videoLibrary;

        public FavoriteScenesViewModel(IVideoLibrary videoLibrary)
        {
            this.videoLibrary = videoLibrary;
        }

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
                };
                favList.AddRange(favScenes);
                if (favList.Count > 0)
                {
                    GroupedFavoriteScenes.Add(favList);
                }
            }
        }

        public ObservableCollection<FavoriteList> GroupedFavoriteScenes { get; } = new ObservableCollection<FavoriteList>();
    }

    public class FavoriteList : List<FavoriteScene>
    {
        public string VideoTitle { get; set; }
        public List<FavoriteScene> Favorites => this;
    }
}
