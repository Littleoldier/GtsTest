using System;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public static class AuditService
    {
        public static void Log(int userId, string username, string actionType, string detail, IDataRepository repo)
        {
            if (repo == null) return;
            Task.Run(() => repo.SaveAuditLogAsync(userId, username, actionType, detail, DateTime.Now));
        }
    }
}