using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VideoPlayerTrimmer.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer.FilePicker
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimpleFilePicker : ContentView
    {
        public SimpleFilePicker()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty InitialFolderPathProperty =
            BindableProperty.Create(nameof(InitialFolderPath),
                                    typeof(string),
                                    typeof(SimpleFilePicker),
                                    default(string),
                                    propertyChanged: InitialFolderPathPropertyChanged);
        public string InitialFolderPath
        {
            get { return (string)GetValue(InitialFolderPathProperty); }
            set { SetValue(InitialFolderPathProperty, value); }
        }

        private static void InitialFolderPathPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is SimpleFilePicker filePicker && newValue is string path)
            {
                filePicker.CurrentFolder = new FolderItem(path);
                filePicker.EnumerateDirectory(path, false);
            }
        }

        public static readonly BindableProperty FileTappedCommandProperty =
            BindableProperty.Create(nameof(FileTappedCommand),
                                    typeof(ICommand),
                                    typeof(SimpleFilePicker),
                                    default(ICommand));
        public ICommand FileTappedCommand
        {
            get { return (ICommand)GetValue(FileTappedCommandProperty); }
            set { SetValue(FileTappedCommandProperty, value); }
        }

        public static readonly BindableProperty FileServiceProperty =
            BindableProperty.Create(nameof(FileService),
                                    typeof(IFileService),
                                    typeof(SimpleFilePicker),
                                    default(IFileService));
        public IFileService FileService
        {
            get { return (IFileService)GetValue(FileServiceProperty); }
            set { SetValue(FileServiceProperty, value); }
        }

        public static readonly BindableProperty CurrentFolderProperty =
            BindableProperty.Create(nameof(CurrentFolder),
                                    typeof(FolderItem),
                                    typeof(SimpleFilePicker),
                                    default(FolderItem));
        public FolderItem CurrentFolder
        {
            get { return (FolderItem)GetValue(CurrentFolderProperty); }
            set { SetValue(CurrentFolderProperty, value); }
        }


        public static readonly BindableProperty AllowedFileExtensionsProperty =
            BindableProperty.Create(nameof(AllowedFileExtensions),
                                    typeof(List<string>),
                                    typeof(SimpleFilePicker),
                                    default(List<string>));
        public List<string> AllowedFileExtensions
        {
            get { return (List<string>)GetValue(AllowedFileExtensionsProperty); }
            set { SetValue(AllowedFileExtensionsProperty, value); }
        }


        private Stack<FolderItem> previousFolders = new Stack<FolderItem>();

        public ObservableCollection<IStorageItem> StorageItems { get; } = new ObservableCollection<IStorageItem>();

        private void GoToPreviousClicked(object sender, EventArgs e)
        {
            GoToPrevious();
        }

        private void GoToParentClicked(object sender, EventArgs e)
        {
            GoToParent();
        }

        private void StorageItemTapped(object sender, EventArgs e)
        {
            if (e is TappedEventArgs args)
            {
                switch (args.Parameter)
                {
                    case FolderItem folder:
                        EnumerateDirectory(folder.Path);
                        break;
                    case FileItem file:
                        Execute(FileTappedCommand, file);
                        break;
                }
            }
        }

        private void EnumerateDirectory(string path, bool addToHistory = true)
        {
            if (FileService != null)
            {
                if(addToHistory)
                {
                    previousFolders.Push(CurrentFolder);
                }
                CurrentFolder = new FolderItem(path);
                var items = FileService.GetStorageItems(path);
                StorageItems.Clear();
                StorageItems.AddRange(items.OfType<FolderItem>());
                var files = items.OfType<FileItem>()
                    .Where(x => AllowedFileExtensions.Count == 0 || AllowedFileExtensions.Contains(x.Extension));
                StorageItems.AddRange(files);
            }
        }

        private void GoToPrevious()
        {
            if (previousFolders.Count > 0)
            {
                var prevFolder = previousFolders.Pop();
                EnumerateDirectory(prevFolder.Path, false);
            }
        }

        private void GoToParent()
        {
            if (FileService != null && FileService.CanGoToParent(CurrentFolder?.Path))
            {
                string parentPath = Path.GetDirectoryName(CurrentFolder.Path);
                EnumerateDirectory(parentPath);
            }
        }


        private void Execute<T>(ICommand command, T parameter)
        {
            if (command == null) return;
            if (command.CanExecute(null))
            {
                command.Execute(parameter);
            }
        }
    }
}