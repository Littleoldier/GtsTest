using System;

namespace GtsTest.Services
{
    public enum AlarmSeverity { Info, Warning, Error, Critical }

    public class AlarmRecord
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string Message { get; set; }
        public AlarmSeverity Severity { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsAcknowledged { get; set; }
        public bool IsResolved { get; set; }
        public string AcknowledgedBy { get; set; }
        public string ResolvedBy { get; set; }
        public DateTime? AcknowledgedTime { get; set; }
        public DateTime? ResolvedTime { get; set; }
    }
}