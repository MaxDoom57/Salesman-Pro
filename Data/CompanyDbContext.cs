using Bridge_App.Models;
using Microsoft.EntityFrameworkCore;
using static Bridge_App.Models.ItemModel;
using static Bridge_App.Models.OrdersModel;
using static Bridge_App.Models.StoresModel;
using static Bridge_App.Models.ValidationModel;

namespace Bridge_App.Data
{
    public class CompanyDbContext : DbContext
    {
        public CompanyDbContext(DbContextOptions<CompanyDbContext> options)
        : base(options) { }

        // Example tables
        public DbSet<vewItmMas> vewItmMas { get; set; }
        public DbSet<Items> Items { get; set; }
        public DbSet<Stores> Stores { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderLines> OrderLines { get; set; }
        public DbSet<UnitCnv> UnitCnv { get; set; }
        public DbSet<CdMas> CdMas { get; set; }
        public DbSet<UsrMas> Users { get; set; }


    }
}
