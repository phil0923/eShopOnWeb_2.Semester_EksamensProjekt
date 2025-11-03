using CommonModels.MessageBroker.Contracts;
using MassTransit;

namespace LagerService.Services
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;
        private readonly IWarehouseService _warehouseService;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IWarehouseService warehouseService)
        {
            _logger = logger;
            _warehouseService = warehouseService;
        }

        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var orderEvent = context.Message;
            await _warehouseService.ReserveStock(orderEvent);

            _logger.LogInformation("Processed order: {OrderId}", orderEvent.OrderId);
            _logger.LogInformation("Event consumed successfully.");
        }
    }
}
