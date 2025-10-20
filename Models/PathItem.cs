
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace fast_cli_tool.Models
{
    public class PathItem : INotifyPropertyChanged
    {
        private string _name;
        private string _fullPath;
        private string _selectedCli;
        private string _customCommand;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCli
        {
            get => _selectedCli;
            set
            {
                _selectedCli = value;
                OnPropertyChanged();
            }
        }

        public string CustomCommand
        {
            get => _customCommand;
            set
            {
                _customCommand = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
