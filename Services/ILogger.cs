using GtsTest;  // 引用 AppLogger 的 LogLevel 枚举
// 文件: Services/ILogger.cs
namespace GtsTest.Services
{
    public interface ILogger
    {
        void Info(string msg, string category = null);
        void Warn(string msg, string category = null);
        void Error(string msg, string category = null);
        void Debug(string msg, string category = null);
        void Fatal(string msg, string category = null);
        void Log(string msg, LogLevel level, string category = null);
    }

}