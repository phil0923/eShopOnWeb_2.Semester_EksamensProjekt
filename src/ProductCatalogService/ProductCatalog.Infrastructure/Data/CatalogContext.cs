using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Entities;

namespace ProductCatalog.Infrastructure.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
        {
        }

        public DbSet<CatalogItem> CatalogItems => Set<CatalogItem>();
        public DbSet<CatalogBrand> CatalogBrands => Set<CatalogBrand>();
        public DbSet<CatalogType> CatalogTypes => Set<CatalogType>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CatalogBrand>().ToTable("CatalogBrands");
            modelBuilder.Entity<CatalogType>().ToTable("CatalogTypes");
            modelBuilder.Entity<CatalogItem>().ToTable("CatalogItems");
        }
    }
}