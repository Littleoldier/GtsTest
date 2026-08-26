// Services/User.cs
using System;

namespace GtsTest.Models
{
    public class User
    {
        // 主键（自增）
        public long Id { get; set; }

        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = ""; // 存储完整哈希字符串（含 salt/iterations）
        public string? Salt { get; set; } // 仅用于兼容旧哈希，新的 PBKDF2 格式不需要单独字段
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "Operator";
        public int IsActive { get; set; } = 1; // 1 = 启用, 0 = 禁用
        public string CreatedTime { get; set; } = DateTime.UtcNow.ToString("o");

        // 可扩展：FailedAttempts、LockoutUntil 等
        public int FailedAttempts { get; set; } = 0;
        public string? LockoutUntil { get; set; } = null;

        // ---- 逻辑删除字段 ----
        public int IsDeleted { get; set; } = 0;         // 0=正常, 1=已删除
        public string? DeletedTime { get; set; }
        public string? DeletedBy { get; set; }
    }
}