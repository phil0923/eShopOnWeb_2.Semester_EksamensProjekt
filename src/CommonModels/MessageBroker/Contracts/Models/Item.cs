using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagerService.Models
{
    public class Item
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public int ReservedStock { get; set; }
        public int Stock { get; set; }
    }
}
