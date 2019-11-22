using AsyncAwaitBestPractices.MVVM;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;

namespace VideoPlayerTrimmer.ViewModels
{
    public class FoldersViewModel : BaseViewModel
    {
        private readonly IVideoLibrary videoLibrary;

        public FoldersViewModel(IVideoLibrary videoLibrary)
        {
            App.DebugLog("");
            this.videoLibrary = videoLibrary;
            Folders = new ObservableCollection<FolderItem>();
            ItemTappedCommand = new AsyncCommand<object>((item) => NavigateToVideosPage(item));
        }

        public ObservableCollection<FolderItem> Folders { get; set; }

        public IAsyncCommand<object> ItemTappedCommand { get; set; }

        protected override async Task InitializeVMAsync(CancellationToken token)
        {
            App.DebugLog("");
            var list = await videoLibrary.GetFolderItemsAsync(true);
            Folders.Clear();
            foreach (var item in list.OrderBy(e => e.FolderName))
            {
                Folders.Add(item);
            }
        }

        public Task NavigateToVideosPage(object item)
        {
            var folderItem = item as FolderItem;
            return NavigateToVideosPage(folderItem.FolderPath);
        }

        public ObservableCollection<VideoItem> Results = new ObservableCollection<VideoItem>();

        public void Search(string query)
        {
            var items = videoLibrary.SearchVideoItems(query);
            Results.Clear();
            foreach(var item in items)
            {
                Results.Add(item);
            }
        }

        public Task OnVideoSelected(object val)
        {
            VideoItem item;
            if (val is VideoItem)
            {
                item = (VideoItem)val;
            }
            else
            {
                item = videoLibrary.SearchVideoItems(val as string).FirstOrDefault();
            }

            return NavigateToVideosPage(item.FolderPath);
        }

        private Task NavigateToVideosPage(string folderPath)
        {
            return App.NavigationService.NavigateToAsync($"{PageNames.Videos}" +
                $"?{NavigationParameterNames.Directory}={folderPath}", false);
        }
    }
}
