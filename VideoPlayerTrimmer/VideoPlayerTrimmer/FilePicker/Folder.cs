namespace VideoPlayerTrimmer.FilePicker
{
    public class FolderItem : IStorageItem
    {
        public FolderItem(string itemPath)
        {
            Path = itemPath;
        }

        public string Path { get; set; }

        public string Name => System.IO.Path.GetFileName(Path);
    }
}
