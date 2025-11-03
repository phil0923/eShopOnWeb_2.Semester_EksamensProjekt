using LagerService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagerService.Database
{
    public class DBContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public DBContext(DbContextOptions<DBContext> options): base(options)
        {
   
        }   
    }
}
