namespace Purchases.Api.Domain.Entities;

public sealed class PurchaseOrderItem
{
    public int Id { get; set; }
    public Guid Cen { get; set; } = Guid.NewGuid();
    public int OrderId { get; set; }
    public Guid OrderCen { get; set; }
    public int ProductId { get; set; }
    public Guid ProductCen { get; set; }
    public decimal Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
