using CommonModels.MessageBroker.Contracts.NewFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.MessageBroker.Contracts
{
    public class OrderCancelledEvent
    {
        public Guid OrderId { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}
