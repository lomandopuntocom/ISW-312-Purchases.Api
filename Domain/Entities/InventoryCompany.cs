namespace Purchases.Api.Domain.Entities;

/// <summary>Read-only mapping for resolving company_id from inventory.companies.</summary>
public sealed class InventoryCompany
{
    public int Id { get; set; }
    public Guid Cen { get; set; }
    public bool Active { get; set; } = true;
}
