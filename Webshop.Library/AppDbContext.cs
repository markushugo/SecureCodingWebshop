using Microsoft.EntityFrameworkCore;
using Webshop.Library.Models;

namespace Webshop.Library
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().HasKey(u => u.Email);
        }
    }
}