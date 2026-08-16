using System;

namespace GtsTest.Services
{
    public static class SessionManager
    {
        private static User _currentUser;
        public static event Action<User> OnUserChanged;

        public static User CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                AppLogger.CurrentUser = value?.Username ?? "未登录";
                OnUserChanged?.Invoke(value);
            }
        }

        public static bool IsLoggedIn => _currentUser != null;

        public static void Login(User user, IDataRepository repo)
        {
            CurrentUser = user;
            AuditService.Log(user.Id, user.Username, "Login", "用户登录成功", repo);
        }

        public static void Logout(IDataRepository repo)
        {
            if (CurrentUser != null)
                AuditService.Log(CurrentUser.Id, CurrentUser.Username, "Logout", "用户注销", repo);
            CurrentUser = null;
        }
    }
}