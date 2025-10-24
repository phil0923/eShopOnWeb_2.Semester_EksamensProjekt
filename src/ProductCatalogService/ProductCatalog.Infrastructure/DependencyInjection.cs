using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Infrastructure.Data;

namespace ProductCatalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCatalogDb(this IServiceCollection services, IConfiguration config)
        {
            var cs = config.GetConnectionString("CatalogConnection")
                        ?? "Server=localhost,1433;Database=CatalogDb_Split;User Id=sa;Password=@someThingComplicated1234;TrustServerCertificate=True";
            services.AddDbContext<CatalogContext>(options =>
                options.UseSqlServer(cs, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Catalog")));
            return services;
        }
    }
}