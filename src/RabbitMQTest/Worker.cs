using CommonModels.MessageBroker.Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace RabbitMQTest;

public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Create a scope to resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var orderEvent = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid(),
            OrderItems =
            {
                new() { ItemId = 1, Quantity = 5 },
                new() { ItemId = 2, Quantity = 3 }
            }
        };

        _logger.LogInformation("Publishing OrderPlacedEvent: {OrderId}", orderEvent.OrderId);
        await publishEndpoint.Publish(orderEvent, stoppingToken);
        _logger.LogInformation("Event published successfully.");
    }
}
