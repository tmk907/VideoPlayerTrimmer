namespace VideoPlayerTrimmer.FilePicker
{
    public interface IStorageItem
    {
        string Path { get; set; }
        string Name { get; }
    }
}
