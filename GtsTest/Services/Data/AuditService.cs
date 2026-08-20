// Services/AuditService.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GtsTest.Core;
using Microsoft.Data.Sqlite;

namespace GtsTest.Services.Data
{
    /// <summary>
    /// 审计服务：把关键操作写入本地 SQLite（gts.db）的 AuditLogs 表。
    /// Log 为非阻塞写入（在线程池中异步执行），并在失败时记录到 AppLogger。
    /// </summary>
    public static class AuditService
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gts.db");
        private static readonly string ConnectionString = new SqliteConnectionStringBuilder { DataSource = DbPath }.ToString();
        private static readonly object _initLock = new object();
        private static bool _tableEnsured = false;

        /// <summary>
        /// 审计记录的数据结构
        /// </summary>
        public class AuditRecord
        {
            public long Id { get; set; }
            public long UserId { get; set; }
            public string Username { get; set; } = "";
            public string ActionType { get; set; } = "";
            public string Detail { get; set; } = "";
            public string Timestamp { get; set; } = "";
        }

        /// <summary>
        /// 写一条审计记录（非阻塞，fire-and-forget）。传入 repo 主要用于上下文/兼容性，可以传 null。
        /// </summary>
        public static void Log(long userId, string username, string actionType, string detail, IDataRepository? repo = null)
        {
            // Run in thread-pool so UI thread isn't blocked by disk I/O
            Task.Run(() =>
            {
                try
                {
                    EnsureTable();

                    using var conn = new SqliteConnection(ConnectionString);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                    INSERT INTO AuditLogs (UserId, Username, ActionType, Detail, Timestamp)
                    VALUES (@uid, @uname, @atype, @detail, @ts);";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@uname", username ?? "");
                    cmd.Parameters.AddWithValue("@atype", actionType ?? "");
                    cmd.Parameters.AddWithValue("@detail", detail ?? "");
                    cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // 记录到 AppLogger（兜底）
                    try { AppLogger.Warn($"无法写入审计日志: {ex.Message}", "AuditService"); } catch { }
                }
            });
        }

        /// <summary>
        /// 异步写入（如果调用方希望等待写入完成可以使用此方法）
        /// </summary>
        public static async Task LogAsync(long userId, string username, string actionType, string detail, IDataRepository? repo = null)
        {
            try
            {
                EnsureTable();

                using var conn = new SqliteConnection(ConnectionString);
                await conn.OpenAsync().ConfigureAwait(false);
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                INSERT INTO AuditLogs (UserId, Username, ActionType, Detail, Timestamp)
                VALUES (@uid, @uname, @atype, @detail, @ts);";
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.Parameters.AddWithValue("@uname", username ?? "");
                cmd.Parameters.AddWithValue("@atype", actionType ?? "");
                cmd.Parameters.AddWithValue("@detail", detail ?? "");
                cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                try { AppLogger.Warn($"无法写入审计日志(Async): {ex.Message}", "AuditService"); } catch { }
            }
        }

        /// <summary>
        /// 读取最近的审计记录（按时间降序）。
        /// </summary>
        public static List<AuditRecord> GetRecent(int limit = 200)
        {
            var list = new List<AuditRecord>();
            try
            {
                EnsureTable();

                using var conn = new SqliteConnection(ConnectionString);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                SELECT Id, UserId, Username, ActionType, Detail, Timestamp
                FROM AuditLogs
                ORDER BY Timestamp DESC
                LIMIT @limit;";
                cmd.Parameters.AddWithValue("@limit", limit);
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    list.Add(new AuditRecord
                    {
                        Id = rdr.IsDBNull(0) ? 0 : rdr.GetInt64(0),
                        UserId = rdr.IsDBNull(1) ? 0 : rdr.GetInt64(1),
                        Username = rdr.IsDBNull(2) ? "" : rdr.GetString(2),
                        ActionType = rdr.IsDBNull(3) ? "" : rdr.GetString(3),
                        Detail = rdr.IsDBNull(4) ? "" : rdr.GetString(4),
                        Timestamp = rdr.IsDBNull(5) ? "" : rdr.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                try { AppLogger.Warn($"读取审计日志失败: {ex.Message}", "AuditService"); } catch { }
            }
            return list;
        }

        /// <summary>
        /// 确保 AuditLogs 表存在（线程安全）
        /// </summary>
        private static void EnsureTable()
        {
            if (_tableEnsured) return;
            lock (_initLock)
            {
                if (_tableEnsured) return;
                try
                {
                    using var conn = new SqliteConnection(ConnectionString);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS AuditLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER,
                        Username TEXT,
                        ActionType TEXT,
                        Detail TEXT,
                        Timestamp TEXT
                    );";
                    cmd.ExecuteNonQuery();
                    _tableEnsured = true;
                }
                catch (Exception ex)
                {
                    try { AppLogger.Warn($"初始化审计表失败: {ex.Message}", "AuditService"); } catch { }
                }
            }
        }
    }
}