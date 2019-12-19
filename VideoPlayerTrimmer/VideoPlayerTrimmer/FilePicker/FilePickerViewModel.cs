using System.Collections.Generic;
using VideoPlayerTrimmer.Framework;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.FilePicker
{
    public class FilePickerViewModel : BindableBase
    {
        public FilePickerViewModel(IFileService fileService)
        {
            FileService = fileService;
        }

        public List<string> Extensions { get; } = new List<string>() { ".srt", ".txt", ".ass" };

        public IFileService FileService { get; private set; }

        private string startupPath;
        public string StartupPath
        {
            get { return startupPath; }
            set { SetProperty(ref startupPath, value); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public Command<object> SubtitleFileTappedCommand { get; set; }

        private bool isOpen;
        public bool IsOpen
        {
            get { return isOpen; }
            set { SetProperty(ref isOpen, value); }
        }
    }
}
