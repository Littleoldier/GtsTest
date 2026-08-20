using System;

namespace GtsTest.Services.Data
{
    public enum AlarmSeverity { Info, Warning, Error, Critical }

    public class AlarmRecord
    {
        public int Id { get; set; }//主键，自增
        public string DeviceId { get; set; }//关联设备 ID
        public string Message { get; set; }//报警消息
        public AlarmSeverity Severity { get; set; }//严重等级
        public DateTime Timestamp { get; set; }//触发时间（ISO 8601）
        public bool IsAcknowledged { get; set; }//是否已确认
        public bool IsResolved { get; set; }//是否已解决
        public string AcknowledgedBy { get; set; }//确认人
        public string ResolvedBy { get; set; }//解决人
        public DateTime? AcknowledgedTime { get; set; }//确认时间
        public DateTime? ResolvedTime { get; set; }//解决时间
    }
}