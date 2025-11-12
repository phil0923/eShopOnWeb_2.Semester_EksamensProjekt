using LagerService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels.MessageBroker.Contracts
{
    public class CatalogItemsEvent
    {
        public DateTime CreatedAt = DateTime.Now;
        public List<Item> Items { get; set; } = new();
    }
}
