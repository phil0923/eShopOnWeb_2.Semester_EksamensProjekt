using LagerService.Database;
using LagerService.Models;
using LagerService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Serilog;
using Serilog.Sinks.OpenSearch;
using System.Reflection;


var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", environment)
    .Enrich.WithProperty("Application", assemblyName)
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{assemblyName.ToLowerInvariant()}-logs-{environment?.ToLowerInvariant().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
        NumberOfReplicas = 1,
        NumberOfShards = 1
    })
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register warehouse service
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddDbContext<DBContext>(options =>
        options.UseInMemoryDatabase("LagerDB"));

        // MassTransit setup
        services.AddMassTransit(x =>
        {
            x.AddConsumer<OrderPlacedConsumer>();
            x.AddConsumer<OrderCancelledConsumer>();
            x.AddConsumer<NewStockConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("order_placed", e =>
                {
                    e.ConfigureConsumer<OrderPlacedConsumer>(ctx);
                });
                cfg.ReceiveEndpoint("order_cancelled", e =>
                {
                    e.ConfigureConsumer<OrderCancelledConsumer>(ctx);
                });
                cfg.ReceiveEndpoint("new_stock", e =>
                {
                    e.ConfigureConsumer<NewStockConsumer>(ctx);
                });
            });
        });

    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DBContext>();

    if (!context.Items.Any())
    {
        var items = new List<Item>();

        for (int i = 1; i <= 100; i++)
        {
            items.Add(new Item
            {
                ItemId = i,
                Name = $"Item {i}",
                Stock = 100,      
                ReservedStock = 10
            });
        }

        context.Items.AddRange(items);
        context.SaveChanges();
    }
}
await host.RunAsync();
