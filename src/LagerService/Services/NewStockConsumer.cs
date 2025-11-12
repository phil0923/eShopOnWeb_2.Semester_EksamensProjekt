using CommonModels.MessageBroker.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LagerService.Services
{
    public class NewStockConsumer : IConsumer<NewStockEvent>
    {
        private readonly ILogger<NewStockConsumer> _logger;
        private readonly IWarehouseService _warehouseService;

        public NewStockConsumer(ILogger<NewStockConsumer> logger, IWarehouseService warehouseService)
        {
            _logger = logger;
            _warehouseService = warehouseService;
        }

        public async Task Consume(ConsumeContext<NewStockEvent> context)
        {
            var stockEvent = context.Message;

            // Call your warehouse service to add stock
            await _warehouseService.AddStock(stockEvent);

            _logger.LogInformation("Stock added for ItemId: {ItemId}", stockEvent.ItemId);
        }
    }
}
