using System.Linq;

namespace WireguardAdmin.Models
{
    public class AdminRepository : IAdminRepository
    {
        private AdminDBContext context;
        public AdminRepository(AdminDBContext ctx)
        {
            context = ctx;
        }
        public IQueryable<User> Users => context.Users;

        public void AddUser(User p)
        {
            context.Add(p);
            context.SaveChanges();
        }
        public void DeleteUser(User p)
        {
            context.Remove(p);
            context.SaveChanges();
        }
    }
}
