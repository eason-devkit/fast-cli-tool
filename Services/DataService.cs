
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using fast_cli_tool.Models;

namespace fast_cli_tool.Services
{
    public class DataService
    {
        private readonly string _filePath;
        private readonly LogService _logService;

        public DataService()
        {
            _logService = new LogService();

            try
            {
                // 使用 AppData 目錄確保有寫入權限
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "FastCliTool"
                );

                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                    _logService.LogInfo($"Created application data folder: {appDataFolder}");
                }

                _filePath = Path.Combine(appDataFolder, "paths.json");
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to initialize DataService", ex);
                // Fallback to current directory
                _filePath = "paths.json";
                _logService.LogWarning($"Using fallback path: {_filePath}");
            }
        }

        public List<PathItem> LoadPaths()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logService.LogInfo($"Paths file not found: {_filePath}");
                    return new List<PathItem>();
                }

                string json = File.ReadAllText(_filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logService.LogWarning("Paths file is empty");
                    return new List<PathItem>();
                }

                var paths = JsonSerializer.Deserialize<List<PathItem>>(json);
                _logService.LogInfo($"Loaded {paths?.Count ?? 0} paths from file");
                return paths ?? new List<PathItem>();
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to load paths from file", ex);
                return new List<PathItem>();
            }
        }

        public void SavePaths(IEnumerable<PathItem> paths)
        {
            try
            {
                var pathList = paths.ToList();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(pathList, options);
                File.WriteAllText(_filePath, json);
                _logService.LogInfo($"Saved {pathList.Count} paths to file: {_filePath}");
            }
            catch (Exception ex)
            {
                _logService.LogError("Failed to save paths to file", ex);
                throw; // Re-throw 讓上層可以處理並顯示錯誤訊息
            }
        }
    }
}
