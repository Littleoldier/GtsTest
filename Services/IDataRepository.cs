using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public interface IDataRepository
    {
        // 用户
        Task<User> GetUserByUsernameAsync(string username);
        // 审计日志
        Task SaveAuditLogAsync(int userId, string username, string actionType, string detail, DateTime timestamp);
        // 生产记录
        Task SaveProductionRecordAsync(string deviceId, int current, int target, DateTime timestamp);
        // 报警记录
        Task SaveAlarmRecordAsync(AlarmRecord record);
        Task UpdateAlarmRecordAsync(AlarmRecord record);
        Task<IEnumerable<AlarmRecord>> GetActiveAlarmsAsync();
        Task<IEnumerable<AlarmRecord>> GetAllAlarmsAsync();
        void Initialize();
    }
}