using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.MessageBroker.Contracts
{
    public class NewStockEvent
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
