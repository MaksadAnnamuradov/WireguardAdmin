using System.Collections.Generic;
using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public interface IAdminRepository
    {
        Task DeleteUser(WireguardUser p);
        Task<List<WireguardUser>> GetAllUsers();
        Task SaveChanges();

    }
}