using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Java.IO;
using VideoPlayerTrimmer.Droid.Services;
using VideoPlayerTrimmer.FilePicker;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileService))]
namespace VideoPlayerTrimmer.Droid.Services
{
    public class FileService : IFileService
    {
        public string GetInternalMemoryRootPath()
        {
            return Environment.ExternalStorageDirectory.Path;
        }

        public bool CanGoToParent(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            File file = new File(path);

            if (file.Parent == "/")
                return false;
            if (file.Path == Environment.ExternalStorageDirectory.Path)
                return false;
            
            return true;
        }

        public IEnumerable<IStorageItem> GetStorageItems(string folderPath)
        {
            var storageItems = new List<IStorageItem>();

            if (string.IsNullOrEmpty(folderPath))
                return storageItems;

            File folder = new File(folderPath);
            File[] files = folder.ListFiles() ?? new File[] { };
            foreach (var file in files.Where(x => !x.IsHidden).OrderBy(x => !x.IsDirectory).ThenBy(x => x.Name))
            {
                if (file.IsDirectory)
                {
                    storageItems.Add(new FolderItem(file.Path));
                }
                else
                {
                    storageItems.Add(new FileItem(file.Path));
                }
            }
            return storageItems;
        }
    }
}