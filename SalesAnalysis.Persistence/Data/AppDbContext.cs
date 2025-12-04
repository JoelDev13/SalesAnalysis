using Microsoft.EntityFrameworkCore;
using SalesAnalysis.Domain.Entities.Db;
using SalesAnalysis.Domain.Entities.Dimensions;

namespace SalesAnalysis.Persistence.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

        // Dimension tables
        public DbSet<DimCustomer> DimCustomers => Set<DimCustomer>();
        public DbSet<DimProduct> DimProducts => Set<DimProduct>();
        public DbSet<DimDate> DimDates => Set<DimDate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedOnAdd();
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).ValueGeneratedOnAdd();
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne<Customer>()
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => new { e.OrderId, e.ProductId });
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne<Order>()
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Product>()
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Dimension configurations
            modelBuilder.Entity<DimCustomer>(entity =>
            {
                entity.HasKey(e => e.CustomerKey);
                entity.Property(e => e.CustomerKey).ValueGeneratedOnAdd();
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Region).HasMaxLength(100);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => new { e.Country, e.City });
            });

            modelBuilder.Entity<DimProduct>(entity =>
            {
                entity.HasKey(e => e.ProductKey);
                entity.Property(e => e.ProductKey).ValueGeneratedOnAdd();
                entity.Property(e => e.ProductName).HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Subcategory).HasMaxLength(100);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.SKU).HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Brand);
            });

            modelBuilder.Entity<DimDate>(entity =>
            {
                entity.HasKey(e => e.DateKey);
                entity.Property(e => e.MonthName).HasMaxLength(20);
                entity.Property(e => e.DayName).HasMaxLength(20);
                entity.Property(e => e.FiscalYear).HasMaxLength(10);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Year);
                entity.HasIndex(e => new { e.Year, e.Month });
                entity.HasIndex(e => new { e.Year, e.Quarter });
            });
        }
    }
}
