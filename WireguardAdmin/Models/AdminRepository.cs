using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WireguardAdmin.Models
{
    public class AdminRepository : IAdminRepository
    {
        private AdminDBContext context;
        public AdminRepository(AdminDBContext ctx)
        {
            context = ctx;
        }

        public async void AddUser(User p)
        {
            await context.AddAsync(p);
            await context.SaveChangesAsync();
        }
        public async void DeleteUser(User p)
        {
            context.Users.Remove(p);
            await context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await context.Users.ToListAsync();
        }
    }
}
