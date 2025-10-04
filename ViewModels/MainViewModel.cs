
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Forms;
using fast_cli_tool.Models;
using fast_cli_tool.Services;

namespace fast_cli_tool.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly LogService _logService;
        private PathItem? _selectedPathItem;

        public ObservableCollection<PathItem> PathItems { get; set; }

        public PathItem? SelectedPathItem
        {
            get => _selectedPathItem;
            set
            {
                _selectedPathItem = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPathCommand { get; }
        public ICommand SelectPathCommand { get; }
        public ICommand ExecuteCommand { get; }
        public ICommand RemovePathCommand { get; }

        public MainViewModel()
        {
            _dataService = new DataService();
            _logService = new LogService();

            _logService.LogInfo("Application started");

            var loadedPaths = _dataService.LoadPaths() ?? new List<PathItem>();
            PathItems = new ObservableCollection<PathItem>(loadedPaths);

            AddPathCommand = new RelayCommand(AddPath);
            SelectPathCommand = new RelayCommand<PathItem>(SelectPath);
            ExecuteCommand = new RelayCommand<PathItem>(Execute);
            RemovePathCommand = new RelayCommand<PathItem>(RemovePath);
        }

        private void SelectPath(PathItem pathItem)
        {
            if (pathItem == null) return;

            try
            {
                _logService.LogInfo($"Selected path: {pathItem.FullPath}");
                SelectedPathItem = pathItem;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error selecting path", ex);
            }
        }

        private void AddPath()
        {
            try
            {
                _logService.LogInfo("Opening folder browser dialog");

                var dialog = new System.Windows.Forms.FolderBrowserDialog();
                dialog.Description = "Select a folder to add";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var fullPath = dialog.SelectedPath;
                    _logService.LogInfo($"Folder selected: {fullPath}");

                    if (string.IsNullOrWhiteSpace(fullPath) || !Directory.Exists(fullPath))
                    {
                        _logService.LogWarning($"Invalid path: {fullPath}");
                        return;
                    }

                    if (PathItems.Any(p => p.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logService.LogWarning($"Path already exists: {fullPath}");
                        return;
                    }

                    var newName = new DirectoryInfo(fullPath).Name;

                    var pathItem = new PathItem
                    {
                        Name = newName,
                        FullPath = fullPath
                    };

                    PathItems.Add(pathItem);
                    _dataService.SavePaths(PathItems);
                    _logService.LogInfo($"Path added successfully: {fullPath}");
                }
                else
                {
                    _logService.LogInfo("Folder selection cancelled");
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Error in AddPath", ex);
                System.Windows.MessageBox.Show(
                    $"Error adding path: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void Execute(PathItem pathItem)
        {
            if (pathItem == null || !Directory.Exists(pathItem.FullPath))
            {
                _logService.LogWarning($"Cannot execute - invalid path item");
                return;
            }

            try
            {
                var cliCommand = pathItem.SelectedCli ?? "gemini.cmd";
                _logService.LogInfo($"Executing {cliCommand} in: {pathItem.FullPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = cliCommand,
                    WorkingDirectory = pathItem.FullPath,
                    UseShellExecute = true,
                    CreateNoWindow = false
                });
                _logService.LogInfo("Command executed successfully");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error executing command in {pathItem.FullPath}", ex);
                System.Windows.MessageBox.Show(
                    $"Error executing command: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void RemovePath(PathItem pathItem)
        {
            if (pathItem == null) return;

            try
            {
                _logService.LogInfo($"Removing path: {pathItem.FullPath}");
                PathItems.Remove(pathItem);
                _dataService.SavePaths(PathItems);
                _logService.LogInfo("Path removed successfully");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error removing path: {pathItem.FullPath}", ex);
                System.Windows.MessageBox.Show(
                    $"Error removing path: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
