using CommonModels.MessageBroker.Contracts;
using MassTransit;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagerService.Services
{
    public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
    {
        private readonly ILogger<OrderCancelledConsumer> _logger;
        private readonly IWarehouseService _warehouseService;

        public OrderCancelledConsumer(ILogger<OrderCancelledConsumer> logger, IWarehouseService warehouseService)
        {
            _logger = logger;
            _warehouseService = warehouseService;
        }

        public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
        {
            var orderEvent = context.Message;

            // Call your business logic
            await _warehouseService.MoveFromReservedStock(orderEvent);

            _logger.LogInformation("Replenished stock for OrderId: {OrderId}", orderEvent.OrderId);
        }
    }
}