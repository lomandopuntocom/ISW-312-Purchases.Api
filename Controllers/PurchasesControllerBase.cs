using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Purchases.Api.Application.Dtos;
using Purchases.Api.Domain.Entities;
using Purchases.Api.Infrastructure.Persistence;

namespace Purchases.Api.Controllers;

public abstract class PurchasesControllerBase(PurchasesDbContext db) : ControllerBase
{
    protected PurchasesDbContext Db { get; } = db;

    protected static bool TryParseCen(string value, out Guid cen) => Guid.TryParse(value, out cen);

    protected async Task<InventoryCompany?> ResolveCompanyAsync(string companyCen)
    {
        if (!TryParseCen(companyCen, out var cen)) return null;

        return await Db.InventoryCompanies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Cen == cen && x.Active);
    }

    protected async Task<string> GenerateSupplierCodeAsync(int companyId)
    {
        var count = await Db.Suppliers.CountAsync(x => x.CompanyId == companyId);
        return $"SUP-{(count + 1):D5}";
    }

    protected static SupplierDto ToSupplierDto(Supplier supplier) =>
        new(supplier.Code, supplier.Code, supplier.Name, supplier.Active);
}
