using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Purchases.Api.Application.Dtos;
using Purchases.Api.Domain.Entities;
using Purchases.Api.Infrastructure.Persistence;

namespace Purchases.Api.Controllers;

[ApiController]
[Route("api/purchases/companies/{companyCen}/suppliers")]
public sealed class SuppliersController(PurchasesDbContext db) : PurchasesControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetSuppliers(string companyCen, [FromQuery] bool activeOnly = true)
    {
        var company = await ResolveCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var query = Db.Suppliers.AsNoTracking().Where(x => x.CompanyId == company.Id);
        if (activeOnly) query = query.Where(x => x.Active);

        var suppliers = await query
            .OrderBy(x => x.Name)
            .Select(x => new SupplierDto(x.Code, x.Code, x.Name, x.Active))
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSupplier(string companyCen, UpsertSupplierRequest request)
    {
        var company = await ResolveCompanyAsync(companyCen);
        if (company is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Supplier name is required.");

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateSupplierCodeAsync(company.Id)
            : request.Code.Trim().ToUpperInvariant();

        var exists = await Db.Suppliers.AnyAsync(x => x.CompanyId == company.Id && x.Code == code);
        if (exists) return Conflict($"Supplier code '{code}' already exists.");

        var supplier = new Supplier
        {
            CompanyId = company.Id,
            Code = code,
            Name = request.Name.Trim(),
            Active = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Db.Suppliers.Add(supplier);
        await Db.SaveChangesAsync();

        return Ok(ToSupplierDto(supplier));
    }

    [HttpGet("{supplierCode}")]
    public async Task<IActionResult> GetSupplier(string companyCen, string supplierCode)
    {
        var company = await ResolveCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var code = supplierCode.Trim().ToUpperInvariant();
        var supplier = await Db.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CompanyId == company.Id && x.Code == code);

        return supplier is null ? NotFound() : Ok(ToSupplierDto(supplier));
    }

    [HttpPut("{supplierCen}")]
    public async Task<IActionResult> UpdateSupplier(string companyCen, string supplierCen, UpdateSupplierContractRequest request)
    {
        var company = await ResolveCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var supplier = await Db.Suppliers
            .FirstOrDefaultAsync(x => x.CompanyId == company.Id && x.Code == supplierCen.Trim().ToUpperInvariant());

        if (supplier is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Supplier name is required.");

        supplier.Name = request.Name.Trim();
        supplier.UpdatedAt = DateTime.UtcNow;

        await Db.SaveChangesAsync();

        return Ok(new SupplierContractDto(supplier.Code, supplier.Name));
    }
}

