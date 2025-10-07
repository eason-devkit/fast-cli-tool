
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace fast_cli_tool.Models
{
    public class AppSettings : INotifyPropertyChanged
    {
        private string _defaultCliCommand = "claude.cmd";

        public string DefaultCliCommand
        {
            get => _defaultCliCommand;
            set
            {
                _defaultCliCommand = value;
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
