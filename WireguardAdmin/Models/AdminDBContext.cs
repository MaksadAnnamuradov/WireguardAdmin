using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WireguardAdmin.Models
{
    public class AdminDBContext : DbContext
    {
        public AdminDBContext(DbContextOptions<AdminDBContext> options)
            : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<NewUserModel> NewUsers { get; set; }
    }
}
