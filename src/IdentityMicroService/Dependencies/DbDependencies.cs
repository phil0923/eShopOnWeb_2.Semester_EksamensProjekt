using Microsoft.EntityFrameworkCore;
using IdentityMicroService.Identity;

namespace IdentityMicroService.Dependencies;

public static class DbDependencies
{
  public static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
  {
    bool useOnlyInMemoryDatabase = true;
    if (configuration["UseOnlyInMemoryDatabase"] != null)
    {
      useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]!);
    }

    if (useOnlyInMemoryDatabase)
    {

      services.AddDbContext<AppIdentityDbContext>(options =>
          options.UseInMemoryDatabase("Identity"));
    }
    else
    {
      services.AddDbContext<AppIdentityDbContext>(options =>
          options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));
    }
  }
}