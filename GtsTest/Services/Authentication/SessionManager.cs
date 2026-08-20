// Services/SessionManager.cs
using GtsTest.Core;
using GtsTest.Models;
using GtsTest.Services.Data;
using System;

namespace GtsTest.Services.Authentication
{
    public static class SessionManager
    {
        private static User? _currentUser;
        public static event Action<User?>? OnUserChanged;

        public static User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                AppLogger.CurrentUser = value?.Username ?? "未登录";
                try
                {
                    OnUserChanged?.Invoke(value);
                }
                catch { /* 订阅者异常不要影响主流程 */ }
            }
        }

        public static bool IsLoggedIn => _currentUser != null;

        public static void Login(User user, IDataRepository repo)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            CurrentUser = user;
            try
            {
                AuditService.Log(user.Id, user.Username, "Login", "用户登录成功", repo);
            }
            catch { }
        }

        public static void Logout(IDataRepository repo)
        {
            try
            {
                if (CurrentUser != null)
                    AuditService.Log(CurrentUser.Id, CurrentUser.Username, "Logout", "用户注销", repo);
            }
            catch { }
            CurrentUser = null;
        }
    }
}