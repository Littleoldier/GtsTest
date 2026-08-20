using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GtsTest.Services.Data;
using Microsoft.Data.Sqlite;

namespace GtsTest.Services.Alarm
{
    public class AlarmManager : IAlarmManager
    {
        private readonly string _connStr;
        private readonly List<AlarmRecord> _alarms = new List<AlarmRecord>();
        private readonly object _lock = new object();
        private bool _loaded = false;

        public event EventHandler<AlarmRecord> AlarmAdded;
        public event EventHandler<int> AlarmAcknowledged;
        public event EventHandler<int> AlarmResolved;

        public AlarmManager()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gts.db");
            _connStr = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
            EnsureTable();
            LoadFromDatabase();
        }

        private void EnsureTable()
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS AlarmRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    DeviceId TEXT,
                    Message TEXT,
                    Severity TEXT,
                    Timestamp TEXT,
                    IsAcknowledged INTEGER,
                    IsResolved INTEGER,
                    AcknowledgedBy TEXT,
                    ResolvedBy TEXT,
                    AcknowledgedTime TEXT,
                    ResolvedTime TEXT
                );
                CREATE INDEX IF NOT EXISTS idx_alarm_timestamp ON AlarmRecords(Timestamp DESC);
                CREATE INDEX IF NOT EXISTS idx_alarm_resolved ON AlarmRecords(IsResolved);
            ";
            cmd.ExecuteNonQuery();
        }

        public void LoadFromDatabase()
        {
            lock (_lock)
            {
                _alarms.Clear();
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT * FROM AlarmRecords WHERE IsResolved = 0 ORDER BY Timestamp DESC;";
                using var rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    _alarms.Add(MapAlarm(rdr));
                }
                _loaded = true;
            }
        }

        public void TriggerAlarm(string deviceId, string message, AlarmSeverity severity)
        {
            var record = new AlarmRecord
            {
                DeviceId = deviceId,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.Now,
                IsAcknowledged = false,
                IsResolved = false
            };

            // 先插入数据库获取 Id
            lock (_lock)
            {
                using var conn = new SqliteConnection(_connStr);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO AlarmRecords (DeviceId, Message, Severity, Timestamp, IsAcknowledged, IsResolved, AcknowledgedBy, ResolvedBy, AcknowledgedTime, ResolvedTime)
                    VALUES (@dev, @msg, @sev, @ts, @ack, @res, @ackBy, @resBy, @ackTime, @resTime);
                    SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@dev", record.DeviceId ?? "");
                cmd.Parameters.AddWithValue("@msg", record.Message);
                cmd.Parameters.AddWithValue("@sev", record.Severity.ToString());
                cmd.Parameters.AddWithValue("@ts", record.Timestamp.ToString("o"));
                cmd.Parameters.AddWithValue("@ack", 0);
                cmd.Parameters.AddWithValue("@res", 0);
                cmd.Parameters.AddWithValue("@ackBy", "");
                cmd.Parameters.AddWithValue("@resBy", "");
                cmd.Parameters.AddWithValue("@ackTime", "");
                cmd.Parameters.AddWithValue("@resTime", "");
                record.Id = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 加入内存列表
            lock (_lock)
            {
                _alarms.Insert(0, record);
            }

            AlarmAdded?.Invoke(this, record);
        }

        public bool AcknowledgeAlarm(int alarmId, string user)
        {
            lock (_lock)
            {
                var alarm = _alarms.FirstOrDefault(a => a.Id == alarmId);
                if (alarm == null || alarm.IsAcknowledged) return false;

                alarm.IsAcknowledged = true;
                alarm.AcknowledgedBy = user;
                alarm.AcknowledgedTime = DateTime.Now;

                UpdateRecord(alarm);
                AlarmAcknowledged?.Invoke(this, alarmId);
                return true;
            }
        }

        public bool ResolveAlarm(int alarmId, string user)
        {
            lock (_lock)
            {
                var alarm = _alarms.FirstOrDefault(a => a.Id == alarmId);
                if (alarm == null || alarm.IsResolved) return false;

                alarm.IsResolved = true;
                alarm.ResolvedBy = user;
                alarm.ResolvedTime = DateTime.Now;

                UpdateRecord(alarm);
                AlarmResolved?.Invoke(this, alarmId);
                return true;
            }
        }

        private void UpdateRecord(AlarmRecord alarm)
        {
            using var conn = new SqliteConnection(_connStr);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE AlarmRecords SET
                    IsAcknowledged = @ack,
                    IsResolved = @res,
                    AcknowledgedBy = @ackBy,
                    ResolvedBy = @resBy,
                    AcknowledgedTime = @ackTime,
                    ResolvedTime = @resTime
                WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@ack", alarm.IsAcknowledged ? 1 : 0);
            cmd.Parameters.AddWithValue("@res", alarm.IsResolved ? 1 : 0);
            cmd.Parameters.AddWithValue("@ackBy", alarm.AcknowledgedBy ?? "");
            cmd.Parameters.AddWithValue("@resBy", alarm.ResolvedBy ?? "");
            cmd.Parameters.AddWithValue("@ackTime", alarm.AcknowledgedTime?.ToString("o") ?? "");
            cmd.Parameters.AddWithValue("@resTime", alarm.ResolvedTime?.ToString("o") ?? "");
            cmd.Parameters.AddWithValue("@id", alarm.Id);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<AlarmRecord> GetActiveAlarms()
        {
            lock (_lock) return _alarms.Where(a => !a.IsResolved).ToList();
        }

        public IEnumerable<AlarmRecord> GetAllAlarms()
        {
            lock (_lock) return _alarms.ToList();
        }

        private AlarmRecord MapAlarm(SqliteDataReader rdr)
        {
            return new AlarmRecord
            {
                Id = rdr.GetInt32(0),
                DeviceId = rdr.IsDBNull(1) ? null : rdr.GetString(1),
                Message = rdr.GetString(2),
                Severity = Enum.Parse<AlarmSeverity>(rdr.GetString(3)),
                Timestamp = DateTime.Parse(rdr.GetString(4)),
                IsAcknowledged = rdr.GetInt32(5) == 1,
                IsResolved = rdr.GetInt32(6) == 1,
                AcknowledgedBy = rdr.IsDBNull(7) ? null : rdr.GetString(7),
                ResolvedBy = rdr.IsDBNull(8) ? null : rdr.GetString(8),
                AcknowledgedTime = rdr.IsDBNull(9) ? null : DateTime.Parse(rdr.GetString(9)),
                ResolvedTime = rdr.IsDBNull(10) ? null : DateTime.Parse(rdr.GetString(10))
            };
        }
    }
}