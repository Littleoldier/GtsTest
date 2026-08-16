using System.Threading.Tasks;

namespace GtsTest.Services
{
    public interface IAuthenticationService
    {
        Task<User> AuthenticateAsync(string username, string password);
        bool HasPermission(User user, string permissionCode);
    }
}