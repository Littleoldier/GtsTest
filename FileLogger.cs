using System;
using System.IO;
using System.Threading;

namespace GtsTest
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static string _logDir = "";
        private static bool _initialized = false;

        public static void Initialize(string logDirectory = "Logs")
        {
            if (_initialized) return;
            _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory);
            if (!Directory.Exists(_logDir))
                Directory.CreateDirectory(_logDir);
            _initialized = true;
        }

        /// <summary>
        /// 写入日志（支持类别：Operation、Monitor、General）
        /// </summary>
        public static void Log(string message, string level = "INFO", string category = "General")
        {
            if (!_initialized) Initialize();

            string date = DateTime.Now.ToString("yyyy-MM-dd");
            // 根据类别生成不同文件名，例如 OperationLog_2026-08-02.txt
            string logFile = Path.Combine(_logDir, $"{category}Log_{date}.txt");
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logLine = $"[{timeStamp}] [{level}] {message}";

            lock (_lock)
            {
                File.AppendAllText(logFile, logLine + Environment.NewLine);
            }
        }
    }
}