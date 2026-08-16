using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public class SqliteRepository : IDataRepository
    {
        private readonly string _connectionString;
        private readonly object _lock = new object();

        public SqliteRepository(string dbFile = "gts.db")
        {
            _connectionString = $"Data Source={dbFile}";
            Initialize();
        }

        public void Initialize()
        {
            lock (_lock)
            {
                using var conn = new SqliteConnection(_connectionString);
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT UNIQUE NOT NULL,
                        PasswordHash TEXT NOT NULL,
                        Salt TEXT NOT NULL,
                        FullName TEXT,
                        Role TEXT NOT NULL,
                        IsActive INTEGER DEFAULT 1,
                        CreatedTime TEXT
                    );
                    CREATE TABLE IF NOT EXISTS AuditLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        Username TEXT,
                        ActionType TEXT NOT NULL,
                        Detail TEXT,
                        Timestamp TEXT NOT NULL
                    );
                    CREATE TABLE IF NOT EXISTS ProductionRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DeviceId TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        CurrentCount INTEGER,
                        TargetCount INTEGER
                    );
                    CREATE TABLE IF NOT EXISTS AlarmRecords (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        DeviceId TEXT,
                        Message TEXT NOT NULL,
                        Severity TEXT NOT NULL,
                        Timestamp TEXT NOT NULL,
                        IsAcknowledged INTEGER DEFAULT 0,
                        IsResolved INTEGER DEFAULT 0,
                        AcknowledgedBy TEXT,
                        ResolvedBy TEXT,
                        AcknowledgedTime TEXT,
                        ResolvedTime TEXT
                    );
                ";
                cmd.ExecuteNonQuery();

                // 插入默认管理员（如果不存在）
                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username='admin'";
                var count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count == 0)
                {
                    var salt = GenerateSalt();
                    var hash = ComputeHash("admin", salt);
                    cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO Users (Username, PasswordHash, Salt, FullName, Role, CreatedTime) VALUES ('admin', @hash, @salt, 'Administrator', 'Admin', @ts)";
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@ts", DateTime.Now.ToString("o"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateSalt() => Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        private string ComputeHash(string password, string salt) =>
            Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(password + salt)));

        // 用户相关
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, Salt, FullName, Role, IsActive FROM Users WHERE Username = @un";
                    cmd.Parameters.AddWithValue("@un", username);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            PasswordHash = reader.GetString(2),
                            Salt = reader.GetString(3),
                            FullName = reader.GetString(4),
                            Role = reader.GetString(5),
                            IsActive = reader.GetInt32(6) == 1
                        };
                    }
                    return null;
                }
            });
        }

        // 审计日志
        public async Task SaveAuditLogAsync(int userId, string username, string actionType, string detail, DateTime timestamp)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO AuditLogs (UserId, Username, ActionType, Detail, Timestamp) VALUES (@uid, @un, @act, @det, @ts)";
                    cmd.Parameters.AddWithValue("@uid", userId);
                    cmd.Parameters.AddWithValue("@un", username ?? "");
                    cmd.Parameters.AddWithValue("@act", actionType);
                    cmd.Parameters.AddWithValue("@det", detail ?? "");
                    cmd.Parameters.AddWithValue("@ts", timestamp.ToString("o"));
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // 生产记录
        public async Task SaveProductionRecordAsync(string deviceId, int current, int target, DateTime timestamp)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO ProductionRecords (DeviceId, Timestamp, CurrentCount, TargetCount) VALUES (@dev, @ts, @cur, @tgt)";
                    cmd.Parameters.AddWithValue("@dev", deviceId);
                    cmd.Parameters.AddWithValue("@ts", timestamp.ToString("o"));
                    cmd.Parameters.AddWithValue("@cur", current);
                    cmd.Parameters.AddWithValue("@tgt", target);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // 报警记录
        public async Task SaveAlarmRecordAsync(AlarmRecord record)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO AlarmRecords 
                        (DeviceId, Message, Severity, Timestamp, IsAcknowledged, IsResolved, AcknowledgedBy, ResolvedBy, AcknowledgedTime, ResolvedTime)
                        VALUES (@dev, @msg, @sev, @ts, @ack, @res, @ackBy, @resBy, @ackTime, @resTime)";
                    cmd.Parameters.AddWithValue("@dev", record.DeviceId ?? "");
                    cmd.Parameters.AddWithValue("@msg", record.Message);
                    cmd.Parameters.AddWithValue("@sev", record.Severity.ToString());
                    cmd.Parameters.AddWithValue("@ts", record.Timestamp.ToString("o"));
                    cmd.Parameters.AddWithValue("@ack", record.IsAcknowledged ? 1 : 0);
                    cmd.Parameters.AddWithValue("@res", record.IsResolved ? 1 : 0);
                    cmd.Parameters.AddWithValue("@ackBy", record.AcknowledgedBy ?? "");
                    cmd.Parameters.AddWithValue("@resBy", record.ResolvedBy ?? "");
                    cmd.Parameters.AddWithValue("@ackTime", record.AcknowledgedTime?.ToString("o") ?? "");
                    cmd.Parameters.AddWithValue("@resTime", record.ResolvedTime?.ToString("o") ?? "");
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public async Task UpdateAlarmRecordAsync(AlarmRecord record)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"UPDATE AlarmRecords SET 
                        IsAcknowledged = @ack, IsResolved = @res, AcknowledgedBy = @ackBy, ResolvedBy = @resBy,
                        AcknowledgedTime = @ackTime, ResolvedTime = @resTime
                        WHERE Id = @id";
                    cmd.Parameters.AddWithValue("@id", record.Id);
                    cmd.Parameters.AddWithValue("@ack", record.IsAcknowledged ? 1 : 0);
                    cmd.Parameters.AddWithValue("@res", record.IsResolved ? 1 : 0);
                    cmd.Parameters.AddWithValue("@ackBy", record.AcknowledgedBy ?? "");
                    cmd.Parameters.AddWithValue("@resBy", record.ResolvedBy ?? "");
                    cmd.Parameters.AddWithValue("@ackTime", record.AcknowledgedTime?.ToString("o") ?? "");
                    cmd.Parameters.AddWithValue("@resTime", record.ResolvedTime?.ToString("o") ?? "");
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public async Task<IEnumerable<AlarmRecord>> GetActiveAlarmsAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<AlarmRecord>();
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM AlarmRecords WHERE IsResolved = 0 ORDER BY Timestamp DESC";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(MapAlarm(reader));
                    }
                }
                return list;
            });
        }

        public async Task<IEnumerable<AlarmRecord>> GetAllAlarmsAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<AlarmRecord>();
                lock (_lock)
                {
                    using var conn = new SqliteConnection(_connectionString);
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM AlarmRecords ORDER BY Timestamp DESC LIMIT 500";
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        list.Add(MapAlarm(reader));
                    }
                }
                return list;
            });
        }

        private AlarmRecord MapAlarm(SqliteDataReader reader)
        {
            return new AlarmRecord
            {
                Id = reader.GetInt32(0),
                DeviceId = reader.IsDBNull(1) ? null : reader.GetString(1),
                Message = reader.GetString(2),
                Severity = Enum.Parse<AlarmSeverity>(reader.GetString(3)),
                Timestamp = DateTime.Parse(reader.GetString(4)),
                IsAcknowledged = reader.GetInt32(5) == 1,
                IsResolved = reader.GetInt32(6) == 1,
                AcknowledgedBy = reader.IsDBNull(7) ? null : reader.GetString(7),
                ResolvedBy = reader.IsDBNull(8) ? null : reader.GetString(8),
                AcknowledgedTime = reader.IsDBNull(9) ? null : DateTime.Parse(reader.GetString(9)),
                ResolvedTime = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10))
            };
        }
    }
}