using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Framework;
using VideoPlayerTrimmer.Models;
using VideoPlayerTrimmer.Services;
using VideoPlayerTrimmer.Views;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.ViewModels
{
    public class FoldersViewModel : BaseViewModel
    {
        private readonly INavigationService navigationService;
        private readonly IVideoLibrary videoLibrary;

        public FoldersViewModel(INavigationService navigationService, IVideoLibrary videoLibrary)
        {
            this.navigationService = navigationService;
            this.videoLibrary = videoLibrary;
            Folders = new ObservableCollection<FolderItem>();
            ItemTappedCommand = new Command<FolderItem>(async (item) => await NavigateToVideosPage(item));
            if (Xamarin.Forms.DesignMode.IsDesignModeEnabled)
            {
                Folders.Add(new FolderItem()
                {
                    FolderName = "Folder1"
                });
                Folders.Add(new FolderItem()
                {
                    FolderName = "Folder2"
                });
            }
        }

        public ObservableCollection<FolderItem> Folders { get; set; }

        public Command ItemTappedCommand { get; set; }

        public override async Task OnAppearingAsync(bool firstTime)
        {
            var list = await videoLibrary.GetFolderItemsAsync(true);
            Folders.Clear();
            foreach (var item in list.OrderBy(e => e.FolderName))
            {
                Folders.Add(item);
            }
        }

        public async Task NavigateToVideosPage(FolderItem item)
        {
            var parameter = new NavigationParameters();
            parameter.Add(NavigationParameterNames.Directory, item.FolderPath);
            await navigationService.NavigateAsync(PageNames.Videos, parameter, false);
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

        public async Task OnVideoSelected(object val)
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

            var parameter = new NavigationParameters();
            parameter.Add(NavigationParameterNames.Directory, item.FolderPath);
            await navigationService.NavigateAsync(PageNames.Videos, parameter, false);
        }
    }
}
