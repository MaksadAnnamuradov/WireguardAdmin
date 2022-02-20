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

        public async Task AddUser(User p)
        {
            await context.AddAsync(p);
            await context.SaveChangesAsync();
        }
        public async Task DeleteUser(User p)
        {
            context.Users.Remove(p);
            await context.SaveChangesAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await context.Users.ToListAsync();
        }

        public async Task AddNewUser(NewUserModel p)
        {
            await context.AddAsync(p);
            await context.SaveChangesAsync();
        }

        public async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }

        public async Task<List<NewUserModel>> GetAllNewUsers()
        {
            return await context.NewUsers.ToListAsync();
        }
    }
}
