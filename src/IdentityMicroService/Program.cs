using Microsoft.AspNetCore.Identity;
using IdentityMicroService.Identity;
using System.Net.Mime;
using Azure.Identity;
using BlazorShared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using IdentityMicroService;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
          //  .AddDefaultUI()
           .AddEntityFrameworkStores<AppIdentityDbContext>()
                           .AddDefaultTokenProviders();

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("IdentityConnection"));
});





// if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker"){
//     // Configure SQL Server (local)
//     IdentityMicroService.Dependencies.DbDependencies.ConfigureServices(builder.Configuration, builder.Services);
// }
// else{
//     // Configure SQL Server (prod)
//     // var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
//     // builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"] ?? ""), credential);
//     // builder.Services.AddDbContext<CatalogContext>(c =>
//     // {
//     //     var connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_CATALOG_CONNECTION_STRING_KEY"] ?? ""];
//     //     c.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
//     // });
//     // builder.Services.AddDbContext<AppIdentityDbContext>(options =>
//     // {
//     //     var connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY"] ?? ""];
//     //     options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
//     // });
// }



var app = builder.Build();

app.MapGet("/", () => "Hello World!");



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Automatically discover all endpoint classes in the assembly
var endpointTypes = typeof(Program).Assembly
    .GetTypes()
    .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces()
        .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEndpoint<>)));

foreach (var type in endpointTypes)
{
  // Use ActivatorUtilities to resolve dependencies (UserManager, etc.)
    var endpoint = (IEndpoint<IResult>)ActivatorUtilities.CreateInstance(app.Services, type);
    endpoint.AddRoute(app);

}






builder.Logging.AddConsole();

app.Run();
