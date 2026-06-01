using Bridge_App.DTOs;
using Bridge_App.Models;
using Microsoft.EntityFrameworkCore;
using static Bridge_App.DTOs.InvoiceDTO;
using static Bridge_App.Models.AccountModel;
using static Bridge_App.Models.CollectionModel;
using static Bridge_App.Models.CompanyModel;
using static Bridge_App.Models.InvoiceModel;
using static Bridge_App.Models.ItemModel;
using static Bridge_App.Models.OrdersModel;
using static Bridge_App.Models.StoresModel;
using static Bridge_App.Models.ValidationModel;
using static Bridge_App.Models.ViewStoresModel;

public class AppDbContext : DbContext
{
    private readonly string? _connectionString;

    // Default constructor for DI-based usage
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Custom constructor for dynamic connection usage
    public AppDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use dynamic connection if available
        if (!optionsBuilder.IsConfigured)
        {
            if (string.IsNullOrEmpty(_connectionString))
                throw new InvalidOperationException("Dynamic connection string is not set for this session.");

            optionsBuilder.UseSqlServer(_connectionString)
                          .EnableDetailedErrors();
        }
    }

    // Master / static tables
    public DbSet<CompanyProject> CompanyProject { get; set; }
    public DbSet<UsrMas> UsrMas { get; set; }
    public DbSet<Company> Company { get; set; }

    // Dynamic / company-specific tables
    public DbSet<Items> Items { get; set; }
    public DbSet<vewItmMas> vewItmMas { get; set; }
    public DbSet<CdMas> CdMas { get; set; }
    public DbSet<UsrMas> Users { get; set; }
    public DbSet<UnitCnv> UnitCnv { get; set; }
    public DbSet<Stores> Stores { get; set; }
    public DbSet<Orders> Orders { get; set; }
    public DbSet<OrderLines> OrderLines { get; set; }
    public DbSet<OrderNumberLast> OrderNumberLast { get; set; }
    public DbSet<ViewStores> ViewStores { get; set; }
    public DbSet<CollectionDt> CollectionDt { get; set; }
    public DbSet<AccountBalanceDetailsDTO> AccountBalanceDetailsDTO { get; set; }
    public DbSet<AddressCdRelModel> AddressCdRelModel { get; set; }
    public DbSet<AccAdrModel> AccAdrModel { get; set; }
    public DbSet<Accounts> Accounts { get; set; }
    public DbSet<TrnMas> TrnMas { get; set; }
    public DbSet<ItmTrn> ItmTrn { get; set; }
    public DbSet<AccTrn> AccTrn { get; set; }
    public DbSet<TrnNoLst> TrnNoLst { get; set; }
    public DbSet<vewTrnNo> vewTrnNo { get; set; }
    public DbSet<AddInvoiceDTO> AddInvoiceDTO { get; set; }
    public DbSet<vewPmtTrmToPrmMode> vewPmtTrmToPrmMode { get; set; }
    public DbSet<vewCdMas> vewCdMas { get; set; }
    public DbSet<UsrAdrRel> UsrAdrRel { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccAdrModel>()
            .HasKey(a => new { a.AccKy, a.AdrKy });

        // DTOs / Views with no keys
        modelBuilder.Entity<AccountBalanceDetailsDTO>().HasNoKey();
        modelBuilder.Entity<AddInvoiceDTO>().HasNoKey();

        // Configure Decimal Precision to resolve EF Core warnings
        
        // Items
        modelBuilder.Entity<Items>(entity => {
            entity.Property(e => e.costPrice).HasPrecision(18, 2);
            entity.Property(e => e.salesPrice).HasPrecision(18, 2);
        });

        // Orders
        modelBuilder.Entity<Orders>(entity => {
            entity.Property(e => e.salesPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderLines>(entity => {
            entity.Property(e => e.costPrice).HasPrecision(18, 2);
            entity.Property(e => e.salesPrice).HasPrecision(18, 2);
            entity.Property(e => e.discountAmount).HasPrecision(18, 2);
        });

        // Invoices (TrnMas / ItmTrn)
        modelBuilder.Entity<TrnMas>(entity => {
            entity.Property(e => e.returnAmount).HasPrecision(18, 2);
            entity.Property(e => e.PmtTrm1Amount).HasPrecision(18, 2);
            entity.Property(e => e.PmtTrm2Amount).HasPrecision(18, 2);
            entity.Property(e => e.amount).HasPrecision(18, 2);
            entity.Property(e => e.discountPresentage).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ItmTrn>(entity => {
            entity.Property(e => e.quantity).HasPrecision(18, 2);
            entity.Property(e => e.discountAmount).HasPrecision(18, 2);
            entity.Property(e => e.discountPresentage).HasPrecision(18, 2);
            entity.Property(e => e.costPrice).HasPrecision(18, 2);
            entity.Property(e => e.salesPrice).HasPrecision(18, 2);
            entity.Property(e => e.tranPrice).HasPrecision(18, 2);
        });

        // Collections
        modelBuilder.Entity<CollectionDt>(entity => {
            entity.Property(e => e.Amt).HasPrecision(18, 2);
        });

        // Account / DTOs
        modelBuilder.Entity<AccountBalanceDetailsDTO>(entity => {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BalAmt).HasPrecision(18, 2);
        });

        modelBuilder.Entity<AddInvoiceDTO>(entity => {
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.totalAmount).HasPrecision(18, 2);
            entity.Property(e => e.discountAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPresentage).HasPrecision(18, 2);
            entity.Property(e => e.ReturnAmount).HasPrecision(18, 2);
            entity.Property(e => e.PmtTrm1Amount).HasPrecision(18, 2);
            entity.Property(e => e.PmtTrm2Amount).HasPrecision(18, 2);
        });

        // Views
        modelBuilder.Entity<vewItmMas>(entity => {
            entity.Property(e => e.costPrice).HasPrecision(18, 2);
            entity.Property(e => e.salesPrice).HasPrecision(18, 2);
        });
    }
}
