using System;
using System.IO;
using GtsTest.Core;
using Microsoft.Data.Sqlite;

namespace GtsTest.Services.Data
{
    public static class ProductionService
    {
        private static readonly string _connStr;
        private static readonly object _lock = new object();
        private static bool _initialized = false;

        static ProductionService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gts.db");
            _connStr = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
            EnsureTable();
        }

        private static void EnsureTable()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ProductionRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DeviceId TEXT,
                        Timestamp TEXT,
                        CurrentCount INTEGER,
                        TargetCount INTEGER
                    );
                    CREATE INDEX IF NOT EXISTS idx_prod_device ON ProductionRecords(DeviceId);
                    CREATE INDEX IF NOT EXISTS idx_prod_time ON ProductionRecords(Timestamp DESC);
                ";
                cmd.ExecuteNonQuery();
                _initialized = true;
            }
        }

        /// <summary>
        /// 异步记录产量更新（非阻塞）
        /// </summary>
        public static void RecordProduction(string deviceId, int current, int target)
        {
            Task.Run(() =>
            {
                try
                {
                    using var conn = new SqliteConnection(_connStr);
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"
                        INSERT INTO ProductionRecords (DeviceId, Timestamp, CurrentCount, TargetCount)
                        VALUES (@dev, @ts, @cur, @tgt);";
                    cmd.Parameters.AddWithValue("@dev", deviceId);
                    cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
                    cmd.Parameters.AddWithValue("@cur", current);
                    cmd.Parameters.AddWithValue("@tgt", target);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    try { AppLogger.Warn($"记录产量失败: {ex.Message}", "Production"); } catch { }
                }
            });
        }
    }
}