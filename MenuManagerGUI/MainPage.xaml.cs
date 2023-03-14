using MenuManagerGUI.ViewModels;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace MenuManagerGUI
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel VM { get; } = new MainViewModel();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btnPickTarget_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".exe");
            openPicker.FileTypeFilter.Add(".bat");

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                VM.CreateMenuItem(file.Path);
            }
        }
    }
}
