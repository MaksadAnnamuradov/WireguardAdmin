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

        public async Task<WireguardUser> GetUserAsync(string userID)
        {
            return await Task.Run(() => context.Users.AsNoTracking().First(r => r.Id == userID));
        }

        public bool UserExists(string userID)
        {
            return context.Users.Any(e => e.Id == userID);
        }


        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}
