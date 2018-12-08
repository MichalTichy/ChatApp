using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace ChatApp.WEB.DAL
{
    public class AppDbContext : IdentityDbContext<ApplicationUser,Role,Guid>
    {
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<GroupMembership> Memberships { get; set; }
        public virtual DbSet<Message> Messages { get; set; }

        public AppDbContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected AppDbContext()
        {
        }
    }
    
}
