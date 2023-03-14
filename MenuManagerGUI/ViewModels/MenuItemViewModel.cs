namespace MenuManagerGUI.ViewModels
{
    public class MenuItemViewModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _target;
        public string Target
        {
            get => _target;
            set => SetProperty(ref _target, value);
        }

        private string _args;
        public string Args
        {
            get => _args;
            set => SetProperty(ref _args, value);
        }
    }
}
