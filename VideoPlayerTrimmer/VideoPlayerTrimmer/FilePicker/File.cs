namespace VideoPlayerTrimmer.FilePicker
{
    public class FileItem : IStorageItem
    {
        public FileItem(string itemPath)
        {
            Path = itemPath;
        }

        public string Path { get; set; }

        public string Extension => System.IO.Path.GetExtension(Path);
        public string FolderPath => System.IO.Path.GetDirectoryName(Path);

        public string Name => System.IO.Path.GetFileName(Path);
    }
}
