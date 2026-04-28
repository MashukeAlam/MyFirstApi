using MyFirstApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MyFirstApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Item> Items { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<InventoryMovement> InventoryMovements { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderLine> PurchaseOrderLines { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<BomHeader> BomHeaders { get; set; }
    public DbSet<BomLine> BomLines { get; set; }
    public DbSet<ProductionOrder> ProductionOrders { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // composite primary key for join table
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<InventoryMovement>()
            .HasOne(m => m.Item)
            .WithMany()
            .HasForeignKey(m => m.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrderLine>()
            .HasOne(l => l.Item)
            .WithMany()
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SalesOrderLine>()
            .HasOne(l => l.Item)
            .WithMany()
            .HasForeignKey(l => l.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BomHeader>()
            .HasOne(b => b.FinishedItem)
            .WithMany()
            .HasForeignKey(b => b.FinishedItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<BomLine>()
            .HasOne(b => b.RawMaterialItem)
            .WithMany()
            .HasForeignKey(b => b.RawMaterialItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }

}