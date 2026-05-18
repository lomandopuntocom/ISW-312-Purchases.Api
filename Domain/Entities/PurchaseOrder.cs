namespace Purchases.Api.Domain.Entities;

public sealed class PurchaseOrder
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int CompanyId { get; set; }
    public Guid CompanyCen { get; set; }
    public string Supplier { get; set; } = string.Empty;
    public Guid? SupplierCen { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "DRAFT";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<PurchaseOrderItem> Items { get; set; } = [];
}
