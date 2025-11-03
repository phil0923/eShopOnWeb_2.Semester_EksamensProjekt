using CommonModels.MessageBroker.Contracts;
using CommonModels.MessageBroker.Contracts.NewFolder;
using LagerService.Database;
using LagerService.Models;
using LagerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace LagerServiceTests
{
    public class LagerServiceTests
    {
        private (WarehouseService service, DBContext context) CreateServiceWithItems(params Item[] items)
        {
            var options = new DbContextOptionsBuilder<DBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new DBContext(options);
            context.Items.AddRange(items);
            context.SaveChanges();

            var service = new WarehouseService(context);
            return (service, context);
        }

        [Fact]
        public async Task AddStock_ShouldIncreaseStock_WhenItemExists()
        {
            // Arrange
            var itemId = 1;
            var (service, context) = CreateServiceWithItems(new Item { ItemId = itemId, Stock = 5 });
            var evt = new NewStockEvent { ItemId = itemId, Quantity = 10 };

            // Act
            await service.AddStock(evt);

            // Assert
            var updated = context.Items.First(i => i.ItemId == itemId);
            Assert.Equal(15, updated.Stock);
        }

        [Fact]
        public async Task AddStock_ShouldThrow_WhenItemNotFound()
        {
            var (service, _) = CreateServiceWithItems();
            var evt = new NewStockEvent { ItemId = 999, Quantity = 5 };

            await Assert.ThrowsAsync<Exception>(() => service.AddStock(evt));
        }

        [Fact]
        public async Task ReserveStock_ShouldReduceStockAndIncreaseReserved_WhenSufficientStock()
        {
            // Arrange
            var itemId = 1;
            var (service, context) = CreateServiceWithItems(new Item { ItemId = itemId, Stock = 10, ReservedStock = 2 });
            var evt = new OrderPlacedEvent
            {
                OrderId = Guid.NewGuid(),
                OrderItems = new List<OrderItem> { new() { ItemId = itemId, Quantity = 4 } }
            };

            // Act
            await service.ReserveStock(evt);

            // Assert
            var updated = context.Items.First(i => i.ItemId == itemId);
            Assert.Equal(6, updated.Stock);
            Assert.Equal(6, updated.ReservedStock);
        }

        [Fact]
        public async Task ReserveStock_ShouldThrow_WhenInsufficientStock()
        {
            // Arrange
            var itemId = 1;
            var (service, _) = CreateServiceWithItems(new Item { ItemId = itemId, Stock = 2, ReservedStock = 0 });
            var evt = new OrderPlacedEvent
            {
                OrderId = Guid.NewGuid(),
                OrderItems = new List<OrderItem> { new() { ItemId = itemId, Quantity = 5 } }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.ReserveStock(evt));
        }

        [Fact]
        public async Task MoveFromReservedStock_ShouldRestoreStockAndReduceReserved()
        {
            // Arrange
            var itemId = 1;
            var (service, context) = CreateServiceWithItems(new Item { ItemId = itemId, Stock = 2, ReservedStock = 5 });
            var evt = new OrderCancelledEvent
            {
                OrderId = Guid.NewGuid(),
                OrderItems = new List<OrderItem> { new() { ItemId = itemId, Quantity = 3 } }
            };

            // Act
            await service.MoveFromReservedStock(evt);

            // Assert
            var updated = context.Items.First(i => i.ItemId == itemId);
            Assert.Equal(5, updated.Stock);
            Assert.Equal(2, updated.ReservedStock);
        }

        [Fact]
        public async Task MoveFromReservedStock_ShouldThrow_WhenItemNotFound()
        {
            // Arrange
            var (service, _) = CreateServiceWithItems();
            var evt = new OrderCancelledEvent
            {
                OrderId = Guid.NewGuid(),
                OrderItems = new List<OrderItem> { new() { ItemId = 999, Quantity = 2 } }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.MoveFromReservedStock(evt));
        }

    }
}