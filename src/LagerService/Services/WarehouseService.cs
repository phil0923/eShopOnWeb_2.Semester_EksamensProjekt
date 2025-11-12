using CommonModels.MessageBroker.Contracts;
using LagerService.Database;

namespace LagerService.Services
{
    public interface IWarehouseService
    {
        Task MoveFromReservedStock(OrderCancelledEvent orderCancelledEvent);
        Task ReserveStock(OrderPlacedEvent orderPlacedEvent);
        Task AddStock(NewStockEvent newStockEvent);
    }

    public class WarehouseService : IWarehouseService
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<WarehouseService>? _logger = null;

        public WarehouseService(DBContext dbContext, ILogger<WarehouseService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public WarehouseService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddStock(NewStockEvent newStockEvent)
        {
            _logger.LogInformation("Received request to add {Quantity} units of Item ID {ItemId}",
                newStockEvent.Quantity, newStockEvent.ItemId);

            var item = _dbContext.Items.FirstOrDefault(i => i.ItemId == newStockEvent.ItemId);
            if (item == null)
            {
                _logger.LogWarning("AddStock failed: Item ID {ItemId} not found", newStockEvent.ItemId);
                throw new Exception($"Item with ID {newStockEvent.ItemId} not found.");
            }

            item.Stock += newStockEvent.Quantity;

            _logger.LogInformation("Successfully added stock: Item ID {ItemId} now has {Stock} units",
                item.ItemId, item.Stock);

            return Task.CompletedTask;
        }

        public Task MoveFromReservedStock(OrderCancelledEvent orderCancelledEvent)
        {
            _logger.LogInformation("Processing order cancellation for Order ID {OrderId} with {Count} items",
                orderCancelledEvent.OrderId, orderCancelledEvent.OrderItems.Count);

            foreach (var orderItem in orderCancelledEvent.OrderItems)
            {
                var item = _dbContext.Items.FirstOrDefault(i => i.ItemId == orderItem.ItemId);
                if (item == null)
                {
                    _logger.LogWarning("MoveFromReservedStock failed: Item ID {ItemId} not found", orderItem.ItemId);
                    throw new Exception($"Item with ID {orderItem.ItemId} not found.");
                }

                item.ReservedStock -= orderItem.Quantity;
                item.Stock += orderItem.Quantity;

                _logger.LogInformation(
                    "Restored {Quantity} units from reserved to available stock for Item ID {ItemId}. New stock: {Stock}, Reserved: {Reserved}",
                    orderItem.Quantity, item.ItemId, item.Stock, item.ReservedStock);
            }

            _logger.LogInformation("Completed processing of OrderCancelledEvent for Order ID {OrderId}", orderCancelledEvent.OrderId);
            return Task.CompletedTask;
        }

        public Task ReserveStock(OrderPlacedEvent orderPlacedEvent)
        {
            _logger.LogInformation("Processing order placement for Order ID {OrderId} with {Count} items",
                orderPlacedEvent.OrderId, orderPlacedEvent.OrderItems.Count);

            foreach (var orderItem in orderPlacedEvent.OrderItems)
            {
                var item = _dbContext.Items.FirstOrDefault(i => i.ItemId == orderItem.ItemId);
                if (item == null)
                {
                    _logger.LogWarning("ReserveStock failed: Item ID {ItemId} not found", orderItem.ItemId);
                    throw new Exception($"Item with ID {orderItem.ItemId} not found.");
                }

                if (item.Stock < orderItem.Quantity)
                {
                    _logger.LogWarning("Insufficient stock for Item ID {ItemId}. Requested {Requested}, Available {Available}",
                        orderItem.ItemId, orderItem.Quantity, item.Stock);
                    throw new Exception($"Insufficient stock for Item ID {orderItem.ItemId}.");
                }

                item.Stock -= orderItem.Quantity;
                item.ReservedStock += orderItem.Quantity;

                _logger.LogInformation(
                    "Reserved {Quantity} units of Item ID {ItemId}. Remaining stock: {Stock}, Reserved: {Reserved}",
                    orderItem.Quantity, item.ItemId, item.Stock, item.ReservedStock);
            }

            _logger.LogInformation("Successfully reserved stock for OrderPlacedEvent Order ID {OrderId}", orderPlacedEvent.OrderId);
            return Task.CompletedTask;
        }
    }
}
