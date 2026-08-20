using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using User = GtsTest.Models.User;

namespace GtsTest.Services.Data
{
    public interface IDataRepository
    {
        // Users
        User? GetUserByUsername(string username);
        bool AddUser(User user);
        bool UpdateUser(User user);
        List<User> GetAllUsers();

    }
}