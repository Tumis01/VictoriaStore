using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VictoriaStore.Api.Models;

namespace VictoriaStore.Api.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistory => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Essential for Identity tables setup

        modelBuilder.Entity<Product>()
            .Property(p => p.Price).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Product>()
            .Property(p => p.SalePrice).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.UnitPrice).HasColumnType("decimal(10,2)");

        modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.IsActive);
        modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();
    }
}