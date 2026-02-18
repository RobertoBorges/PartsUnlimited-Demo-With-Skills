using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PartsUnlimited.Models;

/// <summary>
/// EF Core 8 DbContext. No longer inherits IdentityDbContext —
/// authentication is handled by Entra ID (Microsoft.Identity.Web).
/// </summary>
public class PartsUnlimitedContext : DbContext, IPartsUnlimitedContext
{
    public PartsUnlimitedContext(DbContextOptions<PartsUnlimitedContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Raincheck> RainChecks => Set<Raincheck>();
    public DbSet<Store> Stores => Set<Store>();

    public new EntityEntry Entry(object entity) => base.Entry(entity);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasKey(p => p.ProductId);
        modelBuilder.Entity<Order>().HasKey(o => o.OrderId);
        modelBuilder.Entity<Category>().HasKey(c => c.CategoryId);
        modelBuilder.Entity<CartItem>().HasKey(c => c.CartItemId);
        modelBuilder.Entity<OrderDetail>().HasKey(o => o.OrderDetailId);
        modelBuilder.Entity<Raincheck>().HasKey(r => r.RaincheckId);
        modelBuilder.Entity<Store>().HasKey(s => s.StoreId);

        // Explicit decimal precision to avoid silent SQL truncation (EF Core warning EF30000)
        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Product>().Property(p => p.SalePrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<CartItem>().Property(c => c.UnitPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderDetail>().Property(od => od.UnitPrice).HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId);

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId);

        modelBuilder.Entity<Raincheck>()
            .HasOne(r => r.Store)
            .WithMany()
            .HasForeignKey(r => r.StoreId);

        modelBuilder.Entity<Raincheck>()
            .HasOne(r => r.Product)
            .WithMany()
            .HasForeignKey(r => r.ProductId);

        base.OnModelCreating(modelBuilder);
    }
}
