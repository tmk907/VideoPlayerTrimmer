using System.Collections.Generic;

namespace VideoPlayerTrimmer.FilePicker
{
    public interface IFileService
    {
        string GetInternalMemoryRootPath();
        IEnumerable<IStorageItem> GetStorageItems(string folderPath);
        bool CanGoToParent(string path);
    }
}
