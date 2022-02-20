using System.Collections.Generic;
using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public interface IAdminRepository
    {
        Task AddUser(User p);
        Task DeleteUser(User p);
        Task<List<User>> GetAllUsers();
        Task AddNewUser(NewUserModel p);
        Task<List<NewUserModel>> GetAllNewUsers();
        Task SaveChanges();

    }
}