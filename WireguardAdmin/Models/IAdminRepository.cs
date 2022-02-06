using System.Collections.Generic;
using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public interface IAdminRepository
    {
        void AddUser(User p);
        void DeleteUser(User p);
        Task<List<User>> GetAllUsers();
    }
}