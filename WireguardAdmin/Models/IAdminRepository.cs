using System.Linq;

namespace WireguardAdmin.Models
{
    public interface IAdminRepository
    {
        IQueryable<User> Users { get; }

        void AddUser(User p);
        void DeleteUser(User p);
    }
}