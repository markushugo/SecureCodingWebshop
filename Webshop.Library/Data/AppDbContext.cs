using Microsoft.EntityFrameworkCore;
using Webshop.Library.Models;

namespace Webshop.Library.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguration af modeller kan tilføjes her, hvis nødvendigt.
        }
    }
}