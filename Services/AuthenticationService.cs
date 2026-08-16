using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GtsTest.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDataRepository _repo;
        public AuthenticationService(IDataRepository repo) => _repo = repo;

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            var user = await _repo.GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive) return null;

            var hash = ComputeHash(password, user.Salt);
            if (hash != user.PasswordHash) return null;
            return user;
        }

        private string ComputeHash(string password, string salt) =>
            Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));

        public bool HasPermission(User user, string permissionCode)
        {
            if (user == null) return false;
            if (user.Role == "Admin") return true;

            return (user.Role, permissionCode) switch
            {
                ("Engineer", "Device.Start") => true,
                ("Engineer", "Device.Stop") => true,
                ("Engineer", "Config.Edit") => true,
                ("Engineer", "Config.Delete") => false,
                ("Operator", "Device.Start") => true,
                ("Operator", "Device.Stop") => true,
                ("Operator", "Config.Edit") => false,
                _ => false
            };
        }
    }
}