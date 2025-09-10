using Microsoft.AspNetCore.Identity;
using IdentityMicroService.Identity;
using System.Net.Mime;
using Azure.Identity;
using BlazorShared;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

if (builder.Environment.IsDevelopment() || builder.Environment.EnvironmentName == "Docker"){
    // Configure SQL Server (local)
    IdentityMicroService.Dependencies.DbDependencies.ConfigureServices(builder.Configuration, builder.Services);
}
else{
    // Configure SQL Server (prod)
    // var credential = new ChainedTokenCredential(new AzureDeveloperCliCredential(), new DefaultAzureCredential());
    // builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"] ?? ""), credential);
    // builder.Services.AddDbContext<CatalogContext>(c =>
    // {
    //     var connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_CATALOG_CONNECTION_STRING_KEY"] ?? ""];
    //     c.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
    // });
    // builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    // {
    //     var connectionString = builder.Configuration[builder.Configuration["AZURE_SQL_IDENTITY_CONNECTION_STRING_KEY"] ?? ""];
    //     options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure());
    // });
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
          //  .AddDefaultUI()
           .AddEntityFrameworkStores<AppIdentityDbContext>()
                           .AddDefaultTokenProviders();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
