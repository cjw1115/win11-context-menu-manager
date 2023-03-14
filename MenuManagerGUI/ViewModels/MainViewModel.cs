using System;
using System.Collections.ObjectModel;
using System.IO;
using Windows.Storage;

namespace MenuManagerGUI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<MenuItemViewModel> _menus;
        public ObservableCollection<MenuItemViewModel> Menus
        {
            get => _menus;
            set => SetProperty(ref _menus, value);
        }

        private MenuItemViewModel _currentMenuItem;
        public MenuItemViewModel CurrentMenuItem
        {
            get => _currentMenuItem;
            set => SetProperty(ref _currentMenuItem, value);
        }

        public MainViewModel()
        {
            Menus = new ObservableCollection<MenuItemViewModel>();
        }

        public void AddMenuItem()
        {
            var item = new MenuItemViewModel();
            item.Title = CurrentMenuItem.Title;
            item.Target = CurrentMenuItem.Target;
            item.Args = CurrentMenuItem.Args;

            Menus.Add(item);
        }

        public void CreateMenuItem(string targetPath)
        {
            if (CurrentMenuItem == null)
            {
                CurrentMenuItem = new MenuItemViewModel();
            }
            CurrentMenuItem.Target = targetPath;
        }

        public async void SaveConfigs()
        {
            var config = Newtonsoft.Json.JsonConvert.SerializeObject(Menus);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("menus.json", CreationCollisionOption.ReplaceExisting);
            using(var stream = (await file.OpenAsync(FileAccessMode.ReadWrite)).AsStream())
            {
                var configBytes = System.Text.Encoding.UTF8.GetBytes(config);
                stream.Write(configBytes, 0, configBytes.Length);
            }
        }

        public async void LoadConfigs()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync("menus.json");
                using (var stream = (await file.OpenAsync(FileAccessMode.Read)).AsStream())
                {
                    byte[] buffer = new byte[stream.Length];
                    await stream.ReadAsync(buffer, 0, buffer.Length);
                    var config = System.Text.Encoding.UTF8.GetString(buffer);
                    var menus = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<MenuItemViewModel>>(config);
                    Menus = menus;
                }
            }
            catch
            {
            }
        }
    }
}
