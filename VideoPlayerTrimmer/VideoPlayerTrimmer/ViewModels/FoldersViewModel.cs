using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
            this.videoLibrary = videoLibrary;
            Folders = new ObservableCollection<FolderItem>();
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

        public override async Task InitializeAsync()
        {
            var list = await videoLibrary.GetFolderItemsAsync(true);
            foreach (var item in list)
            {
                Folders.Add(item);
            }
        }
    }
}
