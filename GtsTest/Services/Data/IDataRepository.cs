using System.Collections.Generic;
using GtsTest.Models;

namespace GtsTest.Services.Data
{
    public interface IDataRepository
    {
        // Users
        User? GetUserByUsername(string username);
        bool AddUser(User user);
        bool UpdateUser(User user);
        List<User> GetAllUsers(bool includeDeleted = false);   // 支持查询已删除用户

        // ---- 逻辑删除相关 ----
        bool SoftDeleteUser(long userId, string deletedBy);
        bool RestoreUser(long userId);
    }
}