
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using fast_cli_tool.Models;
using fast_cli_tool.Services;
using Microsoft.Win32;

namespace fast_cli_tool.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly LogService _logService;
        private readonly SettingsService _settingsService;
        private PathItem? _selectedPathItem;
        private string _searchText = string.Empty;
        private ObservableCollection<PathItem> _allPathItems;
        private bool _isSettingsViewActive;
        private string _newCommandText = string.Empty;
        private bool _isAddingCommand;

        public ObservableCollection<PathItem> PathItems { get; set; }

        public AppSettings AppSettings { get; }

        public PathItem? SelectedPathItem
        {
            get => _selectedPathItem;
            set
            {
                _selectedPathItem = value;
                OnPropertyChanged();
                // 當選擇路徑時，自動切換到 Path Details 模式
                if (value != null)
                {
                    IsSettingsViewActive = false;
                }
            }
        }

        public bool IsSettingsViewActive
        {
            get => _isSettingsViewActive;
            set
            {
                _isSettingsViewActive = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterPaths();
            }
        }

        public string NewCommandText
        {
            get => _newCommandText;
            set
            {
                _newCommandText = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddingCommand
        {
            get => _isAddingCommand;
            set
            {
                _isAddingCommand = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddPathCommand { get; }
        public ICommand SelectPathCommand { get; }
        public ICommand ExecuteCommand { get; }
        public ICommand ExecuteCustomCommandCommand { get; }
        public ICommand RemovePathCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowPathDetailsCommand { get; }
        public ICommand StartAddCommandCommand { get; }
        public ICommand ConfirmAddCommandCommand { get; }
        public ICommand CancelAddCommandCommand { get; }
        public ICommand RemoveCustomCommandCommand { get; }

        public MainViewModel()
        {
            _dataService = new DataService();
            _logService = new LogService();
            _settingsService = new SettingsService();
            AppSettings = _settingsService.LoadSettings();

            _logService.LogInfo("Application started");

            var loadedPaths = _dataService.LoadPaths() ?? new List<PathItem>();
            _allPathItems = new ObservableCollection<PathItem>(loadedPaths);
            PathItems = new ObservableCollection<PathItem>(loadedPaths);

            // 監聽每個 PathItem 的屬性變更
            foreach (var item in _allPathItems)
            {
                item.PropertyChanged += PathItem_PropertyChanged;
            }

            // 監聽集合變更
            _allPathItems.CollectionChanged += PathItems_CollectionChanged;

            // 監聽設定變更以自動儲存
            AppSettings.PropertyChanged += AppSettings_PropertyChanged;

            AddPathCommand = new RelayCommand(AddPath);
            SelectPathCommand = new RelayCommand<PathItem>(SelectPath);
            ExecuteCommand = new RelayCommand<PathItem>(Execute);
            ExecuteCustomCommandCommand = new RelayCommand<string>(ExecuteCustomCommand);
            RemovePathCommand = new RelayCommand<PathItem>(RemovePath);
            OpenFolderCommand = new RelayCommand<PathItem>(OpenFolder);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowPathDetailsCommand = new RelayCommand(ShowPathDetails);
            StartAddCommandCommand = new RelayCommand(StartAddCommand);
            ConfirmAddCommandCommand = new RelayCommand(ConfirmAddCommand);
            CancelAddCommandCommand = new RelayCommand(CancelAddCommand);
            RemoveCustomCommandCommand = new RelayCommand<string>(RemoveCustomCommand);
        }

        private void PathItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 新增的項目要監聽屬性變更
            if (e.NewItems != null)
            {
                foreach (PathItem item in e.NewItems)
                {
                    item.PropertyChanged += PathItem_PropertyChanged;
                }
            }

            // 移除的項目要取消監聽
            if (e.OldItems != null)
            {
                foreach (PathItem item in e.OldItems)
                {
                    item.PropertyChanged -= PathItem_PropertyChanged;
                }
            }
        }

        private void PathItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 當 PathItem 的任何屬性變更時，自動保存
            try
            {
                _dataService.SavePaths(_allPathItems);
                _logService.LogInfo($"PathItem property '{e.PropertyName}' changed, data saved");

                // 如果是 Name 或 FullPath 變更，需要重新篩選
                if (e.PropertyName == nameof(PathItem.Name) || e.PropertyName == nameof(PathItem.FullPath))
                {
                    FilterPaths();
                }
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error saving paths after property change", ex);
            }
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 當設定變更時，自動保存
            try
            {
                _settingsService.SaveSettings(AppSettings);
                _logService.LogInfo($"Settings property '{e.PropertyName}' changed, settings saved");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error saving settings after property change", ex);
            }
        }

        private void ShowSettings()
        {
            try
            {
                _logService.LogInfo("Showing settings view");
                IsSettingsViewActive = true;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error showing settings", ex);
            }
        }

        private void ShowPathDetails()
        {
            try
            {
                _logService.LogInfo("Showing path details view");
                IsSettingsViewActive = false;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error showing path details", ex);
            }
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

                var dialog = new OpenFolderDialog();
                dialog.Title = "Select a folder to add";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == true)
                {
                    var fullPath = dialog.FolderName;
                    _logService.LogInfo($"Folder selected: {fullPath}");

                    if (string.IsNullOrWhiteSpace(fullPath) || !Directory.Exists(fullPath))
                    {
                        _logService.LogWarning($"Invalid path: {fullPath}");
                        return;
                    }

                    if (_allPathItems.Any(p => p.FullPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logService.LogWarning($"Path already exists: {fullPath}");
                        return;
                    }

                    var newName = new DirectoryInfo(fullPath).Name;

                    var pathItem = new PathItem
                    {
                        Name = newName,
                        FullPath = fullPath,
                        SelectedCli = AppSettings.DefaultCliCommand
                    };

                    _allPathItems.Add(pathItem);
                    _dataService.SavePaths(_allPathItems);
                    FilterPaths(); // 重新篩選以顯示新項目
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
                var cliCommand = pathItem.SelectedCli ?? AppSettings.DefaultCliCommand;
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

        private void ExecuteCustomCommand(string command)
        {
            if (SelectedPathItem == null || !Directory.Exists(SelectedPathItem.FullPath))
            {
                _logService.LogWarning($"Cannot execute custom command - invalid path item");
                return;
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                _logService.LogWarning($"Cannot execute custom command - command is empty");
                return;
            }

            try
            {
                _logService.LogInfo($"Executing custom command '{command}' in: {SelectedPathItem.FullPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/k cd /d \"{SelectedPathItem.FullPath}\" && {command}",
                    UseShellExecute = true,
                    CreateNoWindow = false
                });
                _logService.LogInfo("Custom command executed successfully");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error executing custom command in {SelectedPathItem.FullPath}", ex);
                System.Windows.MessageBox.Show(
                    $"Error executing custom command: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void StartAddCommand()
        {
            try
            {
                _logService.LogInfo("Starting to add new custom command");
                IsAddingCommand = true;
                NewCommandText = string.Empty;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error starting add command", ex);
            }
        }

        private void ConfirmAddCommand()
        {
            if (SelectedPathItem == null)
            {
                _logService.LogWarning("Cannot add command - no path item selected");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewCommandText))
            {
                _logService.LogWarning("Cannot add command - command text is empty");
                CancelAddCommand();
                return;
            }

            try
            {
                _logService.LogInfo($"Adding custom command: {NewCommandText}");

                if (SelectedPathItem.CustomCommands == null)
                {
                    SelectedPathItem.CustomCommands = new System.Collections.ObjectModel.ObservableCollection<string>();
                }

                SelectedPathItem.CustomCommands.Add(NewCommandText);

                _dataService.SavePaths(_allPathItems);
                _logService.LogInfo("Custom command added successfully");

                IsAddingCommand = false;
                NewCommandText = string.Empty;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error adding custom command", ex);
                System.Windows.MessageBox.Show(
                    $"Error adding command: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void CancelAddCommand()
        {
            try
            {
                _logService.LogInfo("Cancelled adding custom command");
                IsAddingCommand = false;
                NewCommandText = string.Empty;
            }
            catch (Exception ex)
            {
                _logService.LogError("Error cancelling add command", ex);
            }
        }

        private void RemoveCustomCommand(string command)
        {
            if (SelectedPathItem == null)
            {
                _logService.LogWarning("Cannot remove command - no path item selected");
                return;
            }

            if (string.IsNullOrWhiteSpace(command))
            {
                _logService.LogWarning("Cannot remove command - command is empty");
                return;
            }

            try
            {
                _logService.LogInfo($"Removing custom command: {command}");

                if (SelectedPathItem.CustomCommands != null && SelectedPathItem.CustomCommands.Contains(command))
                {
                    SelectedPathItem.CustomCommands.Remove(command);
                    _dataService.SavePaths(_allPathItems);
                    _logService.LogInfo("Custom command removed successfully");
                }
            }
            catch (Exception ex)
            {
                _logService.LogError("Error removing custom command", ex);
                System.Windows.MessageBox.Show(
                    $"Error removing command: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
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
                var result = System.Windows.MessageBox.Show(
                    $"確定要移除「{pathItem.Name}」嗎？",
                    "確認刪除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Question
                );

                if (result != System.Windows.MessageBoxResult.Yes) return;

                _logService.LogInfo($"Removing path: {pathItem.FullPath}");
                _allPathItems.Remove(pathItem);
                _dataService.SavePaths(_allPathItems);
                FilterPaths(); // 重新篩選以更新顯示
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

        private void OpenFolder(PathItem pathItem)
        {
            if (pathItem == null || !Directory.Exists(pathItem.FullPath))
            {
                _logService.LogWarning($"Cannot open folder - invalid path item");
                return;
            }

            try
            {
                _logService.LogInfo($"Opening folder: {pathItem.FullPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = pathItem.FullPath,
                    UseShellExecute = true
                });
                _logService.LogInfo("Folder opened successfully");
            }
            catch (Exception ex)
            {
                _logService.LogError($"Error opening folder: {pathItem.FullPath}", ex);
                System.Windows.MessageBox.Show(
                    $"Error opening folder: {ex.Message}\n\nCheck log file at:\n{_logService.GetLogFilePath()}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
            }
        }

        private void FilterPaths()
        {
            try
            {
                PathItems.Clear();

                if (string.IsNullOrWhiteSpace(_searchText))
                {
                    // 顯示全部
                    foreach (var item in _allPathItems)
                    {
                        PathItems.Add(item);
                    }
                }
                else
                {
                    // 只搜尋資料夾名稱
                    var filtered = _allPathItems.Where(p =>
                        p.Name?.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ?? false
                    );

                    foreach (var item in filtered)
                    {
                        PathItems.Add(item);
                    }
                }

                _logService.LogInfo($"Filtered paths: {PathItems.Count} of {_allPathItems.Count} (search: '{_searchText}')");
            }
            catch (Exception ex)
            {
                _logService.LogError("Error filtering paths", ex);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
