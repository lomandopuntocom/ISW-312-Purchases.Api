using Microsoft.EntityFrameworkCore;
using Purchases.Api.Domain.Entities;

namespace Purchases.Api.Infrastructure.Persistence;

public sealed class PurchasesDbContext(DbContextOptions<PurchasesDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseOrder> Orders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> OrderItems => Set<PurchaseOrderItem>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<InventoryCompany> InventoryCompanies => Set<InventoryCompany>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("purchases");

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.CompanyCen).HasColumnName("company_cen");
            entity.Property(x => x.Supplier).HasColumnName("supplier");
            entity.Property(x => x.SupplierCen).HasColumnName("supplier_cen");
            entity.Property(x => x.Date).HasColumnName("date");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Cen).IsUnique();
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.OrderCen).HasColumnName("order_cen");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.ProductCen).HasColumnName("product_cen");
            entity.Property(x => x.Quantity).HasColumnName("quantity").HasPrecision(12, 2);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers", tb => tb.ExcludeFromMigrations());
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            entity.Property(x => x.CompanyId).HasColumnName("company_id");
            entity.Property(x => x.Code).HasColumnName("code");
            entity.Property(x => x.Name).HasColumnName("name");
            entity.Property(x => x.Active).HasColumnName("active");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<InventoryCompany>(entity =>
        {
            entity.ToTable("companies", "inventory", tb => tb.ExcludeFromMigrations());
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Cen).HasColumnName("cen");
            entity.Property(x => x.Active).HasColumnName("active");
        });
    }
}
