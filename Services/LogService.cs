
using System;
using System.IO;

namespace fast_cli_tool.Services
{
    public class LogService
    {
        private readonly string _logFilePath;
        private static readonly object _lock = new object();

        public LogService()
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FastCliTool"
            );

            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _logFilePath = Path.Combine(appDataFolder, "app.log");
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogError(string message, Exception? ex = null)
        {
            var fullMessage = ex != null
                ? $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}"
                : message;
            WriteLog("ERROR", fullMessage);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        private void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var logEntry = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logEntry);
                }
                catch
                {
                    // 如果寫入日誌失敗，靜默處理避免造成更多問題
                }
            }
        }

        public string GetLogFilePath() => _logFilePath;
    }
}
