using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public class AlarmManager : IAlarmManager
    {
        private readonly IDataRepository _repo;
        private readonly List<AlarmRecord> _alarms = new List<AlarmRecord>();
        private readonly object _lock = new object();
        private bool _loaded = false;

        public event EventHandler<AlarmRecord> AlarmAdded;
        public event EventHandler<int> AlarmAcknowledged;
        public event EventHandler<int> AlarmResolved;

        public AlarmManager(IDataRepository repo)
        {
            _repo = repo;
            // 延迟加载，不阻塞UI线程
        }

        private void EnsureLoaded()
        {
            if (!_loaded)
            {
                lock (_lock)
                {
                    if (!_loaded)
                    {
                        LoadFromDatabase();
                        _loaded = true;
                    }
                }
            }
        }

        public void LoadFromDatabase()
        {
            lock (_lock)
            {
                _alarms.Clear();
                var active = _repo.GetActiveAlarmsAsync().Result; // 这里仍然同步，但只执行一次
                _alarms.AddRange(active);
            }
        }

        public void TriggerAlarm(string deviceId, string message, AlarmSeverity severity)
        {
            EnsureLoaded();
            var record = new AlarmRecord
            {
                DeviceId = deviceId,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.Now,
                IsAcknowledged = false,
                IsResolved = false
            };
            lock (_lock)
            {
                _alarms.Insert(0, record); // 最新在上
            }
            _repo.SaveAlarmRecordAsync(record); // 异步保存
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
                _repo.UpdateAlarmRecordAsync(alarm);
                AlarmAcknowledged?.Invoke(this, alarmId);
                return true;
            }
        }

        public bool ResolveAlarm(int alarmId, string user)
        {
            EnsureLoaded();
            lock (_lock)
            {
                var alarm = _alarms.FirstOrDefault(a => a.Id == alarmId);
                if (alarm == null || alarm.IsResolved) return false;
                alarm.IsResolved = true;
                alarm.ResolvedBy = user;
                alarm.ResolvedTime = DateTime.Now;
                _repo.UpdateAlarmRecordAsync(alarm);
                AlarmResolved?.Invoke(this, alarmId);
                return true;
            }
        }

        public IEnumerable<AlarmRecord> GetActiveAlarms()
        {
            EnsureLoaded();
            lock (_lock) return _alarms.Where(a => !a.IsResolved).ToList();
        }

        public IEnumerable<AlarmRecord> GetAllAlarms()
        {
            EnsureLoaded();
            lock (_lock) return _alarms.ToList();
        }
    }
}