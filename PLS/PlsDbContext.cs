using Microsoft.EntityFrameworkCore;

namespace PLS
{
    public class PlsDbContext : DbContext
    {
        public PlsDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Server> Servers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }

}