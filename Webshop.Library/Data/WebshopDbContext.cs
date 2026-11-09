using Microsoft.EntityFrameworkCore;

namespace Webshop.Library.Data
{
    public class WebshopDbContext : DbContext
    {
        public WebshopDbContext(DbContextOptions<WebshopDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfiguration af modeller vil blive tilføjet her
        }
    }
}