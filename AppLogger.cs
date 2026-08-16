using System;
using System.IO;
using System.Threading;

namespace GtsTest
{
    /// <summary>
    /// 日志级别（数值越大越严重）
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }

    /// <summary>
    /// 工业级日志管理器（单例）
    /// </summary>
    public static class AppLogger
    {
        // ---------- 配置参数（支持运行时修改） ----------
        public static LogLevel GlobalLogLevel { get; set; } = LogLevel.Info; // 运行时动态调整
        public static int MaxFileSizeMB { get; set; } = 10;                  // 单文件最大 10MB
        public static int RetentionDays { get; set; } = 30;                 // 保留最近 30 天
        public static string LogDirectory { get; private set; } = "";

        // ---------- 事件（推送到 UI） ----------
        public static event Action<LogLevel, string, string?>? OnLogReceived; // 参数：级别, 消息, 类别

        private static readonly object _lock = new();
        private static string _currentLogFile = "";
        private static DateTime _currentDate = DateTime.MinValue;
        private static long _currentFileSize = 0;
        public static string CurrentUser { get; set; } = "未登录";
        /// <summary>
        /// 初始化（在 Program.cs 中调用）
        /// </summary>
        public static void Initialize(string logDirectory = "Logs")
        {
            LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDirectory);
            if (!Directory.Exists(LogDirectory))
                Directory.CreateDirectory(LogDirectory);

            // 清理过期日志（启动时执行一次）
            CleanupOldLogs();

            // 准备当天的日志文件
            PrepareLogFile();
        }

        /// <summary>
        /// 核心日志记录方法
        /// </summary>
        public static void Log(string message, LogLevel level = LogLevel.Info, string? category = null)
        {
            // 1. 级别过滤（低于全局级别的直接丢弃）
            if (level < GlobalLogLevel) return;

            // 2. 构造日志行（包含时间、级别、类别）
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string levelStr = level.ToString().ToUpper();
            string categoryTag = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
            string userTag = string.IsNullOrEmpty(CurrentUser) ? "" : $" [User:{CurrentUser}]";
            string logLine = $"[{timeStamp}] [{levelStr}] {categoryTag}{message}";
            

            // 3. 触发 UI 事件（主线程安全由调用方处理）
            OnLogReceived?.Invoke(level, logLine, category);

            // 4. 写入文件（带滚动机制）
            WriteToFile(logLine);
        }

        // ---------- 私有方法 ----------
        private static void PrepareLogFile()
        {
            DateTime today = DateTime.Today;
            _currentDate = today;
            string dateStr = today.ToString("yyyy-MM-dd");
            _currentLogFile = Path.Combine(LogDirectory, $"AppLog_{dateStr}.txt");

            // 如果文件已存在，获取当前大小（用于追加）
            if (File.Exists(_currentLogFile))
            {
                _currentFileSize = new FileInfo(_currentLogFile).Length;
            }
            else
            {
                _currentFileSize = 0;
            }
        }

        private static void WriteToFile(string logLine)
        {
            lock (_lock)
            {
                try
                {
                    // 检查日期是否切换（跨天则新建文件）
                    if (DateTime.Today != _currentDate)
                    {
                        PrepareLogFile();
                    }

                    // 检查文件大小是否超限（滚动切割）
                    if (_currentFileSize > MaxFileSizeMB * 1024 * 1024)
                    {
                        RolloverLogFile();
                    }

                    // 追加写入
                    File.AppendAllText(_currentLogFile, logLine + Environment.NewLine);
                    _currentFileSize += logLine.Length + Environment.NewLine.Length;
                }
                catch (Exception ex)
                {
                    // 日志系统本身崩溃时的最后保障（写入Windows事件日志或控制台）
                    System.Diagnostics.Debug.WriteLine($"日志写入失败: {ex.Message}");
                }
            }
        }

        private static void RolloverLogFile()
        {
            // 重命名当前文件：AppLog_2026-08-08_001.txt, _002.txt ...
            string baseName = Path.GetFileNameWithoutExtension(_currentLogFile); // AppLog_2026-08-08
            string dir = Path.GetDirectoryName(_currentLogFile)!;
            int index = 1;
            string newFile;
            do
            {
                newFile = Path.Combine(dir, $"{baseName}_{index:D3}.txt");
                index++;
            } while (File.Exists(newFile));

            File.Move(_currentLogFile, newFile);
            _currentFileSize = 0; // 重置大小

            // 可选：压缩旧文件（调用 7zip 或 .NET ZipFile）
            // 这里简化，只做重命名
        }

        private static void CleanupOldLogs()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-RetentionDays);
                var files = Directory.GetFiles(LogDirectory, "*.txt");
                foreach (var file in files)
                {
                    if (File.GetCreationTime(file) < cutoff)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch { /* 清理失败不影响主程序 */ }
        }

        // ---------- 便捷方法（兼容旧代码） ----------
        public static void Info(string msg, string? category = null) => Log(msg, LogLevel.Info, category);
        public static void Warn(string msg, string? category = null) => Log(msg, LogLevel.Warn, category);
        public static void Error(string msg, string? category = null) => Log(msg, LogLevel.Error, category);
        public static void Debug(string msg, string? category = null) => Log(msg, LogLevel.Debug, category);
        public static void Fatal(string msg, string? category = null) => Log(msg, LogLevel.Fatal, category);
    }
}