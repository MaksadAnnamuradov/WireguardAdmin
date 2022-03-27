using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WireguardAdmin.Models
{
    public class AdminDBContext : IdentityDbContext<WireguardUser>
    {
        public AdminDBContext(DbContextOptions<AdminDBContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        //public DbSet<User> Users { get; set; }
        //public DbSet<NewUserModelDbo> NewUsers { get; set; }
    }
}
