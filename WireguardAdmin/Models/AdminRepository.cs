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
        public async Task DeleteUser(WireguardUser p)
        {
            context.Users.Remove(p);
            await context.SaveChangesAsync();
        }

        public async Task<List<WireguardUser>> GetAllUsers()
        {
            return await context.Users.ToListAsync();
        }

        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}
