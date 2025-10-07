
using System;
using System.IO;
using System.Text.Json;
using fast_cli_tool.Models;

namespace fast_cli_tool.Services
{
    public class SettingsService
    {
        private readonly string _filePath;
        private readonly LogService _logService;

        public SettingsService()
        {
            _logService = new LogService();

            try
            {
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FastCliTool"
                );

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                    _logService.LogInfo($"Created application data folder: {appDataFolder}");
                }

                _filePath = Path.Combine(appDataFolder, "settings.json");
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to initialize SettingsService", ex);
                _filePath = "settings.json";
                _logService.LogWarning($"Using fallback path: {_filePath}");
            }
        }

        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logService.LogInfo($"Settings file not found: {_filePath}, creating default settings");
                    var defaultSettings = new AppSettings();
                    SaveSettings(defaultSettings);
                    return defaultSettings;
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logService.LogWarning("Settings file is empty, using default settings");
                    return new AppSettings();
                }

                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                _logService.LogInfo($"Loaded settings from file: DefaultCliCommand={settings?.DefaultCliCommand}");
                return settings ?? new AppSettings();
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to load settings from file", ex);
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_filePath, json);
                _logService.LogInfo($"Saved settings to file: {_filePath}");
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to save settings to file", ex);
                throw;
            }
        }
    }
}
