using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Services;

namespace ProductCatalog.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCatalogInfrastructure(
            this IServiceCollection services,
            IConfiguration config)
        {
            var useInMemory = config.GetValue<bool>("UseOnlyInMemoryDatabase");

            if (useInMemory)
            {
                services.AddDbContext<CatalogContext>(o => o.UseInMemoryDatabase("CatalogDb"));
            }
            else
            {
                var cs = config.GetConnectionString("CatalogConnection");
                services.AddDbContext<CatalogContext>(o => o.UseSqlServer(cs));
            }

            services.AddScoped<ICatalogFacade, CatalogFacadeEf>();

            return services;
        }
    }
}
