using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Purchases.Api.Application.Abstractions;
using Purchases.Api.Application.Dtos;
using Purchases.Api.Domain.Entities;
using Purchases.Api.Infrastructure.Persistence;

namespace Purchases.Api.Controllers;

[ApiController]
[Route("api/purchases/companies/{companyCen}/orders")]
public sealed class PurchaseOrdersController(PurchasesDbContext db, IInventoryStockClient inventoryStockClient) : PurchasesControllerBase(db)
{
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        string companyCen,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool sortDescending = true)
    {
        if (!TryParseCen(companyCen, out var company)) return NotFound();

        var query = Db.Orders.Include(x => x.Items).Where(x => x.CompanyCen == company);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status);
        query = sortDescending ? query.OrderByDescending(x => x.Date) : query.OrderBy(x => x.Date);

        var total = await query.CountAsync();
        var items = await query
            .Skip((Math.Max(page, 1) - 1) * Math.Max(pageSize, 1))
            .Take(Math.Max(pageSize, 1))
            .Select(x => new PurchaseOrderListDto(x.Cen.ToString(), x.Supplier, x.Status, x.Date, x.Items.Count))
            .ToListAsync();

        return Ok(new PagedResultDto<PurchaseOrderListDto>(items, total, page, pageSize));
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(string companyCen, CreatePurchaseOrderRequest request)
    {
        if (!TryParseCen(companyCen, out var companyCenValue)) return NotFound();

        var company = await ResolveCompanyAsync(companyCen);
        if (company is null) return NotFound();

        var supplierCode = (request.SupplierCen ?? request.Supplier ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(supplierCode))
            return BadRequest("Supplier code (supplierCen) is required.");

        var supplier = await Db.Suppliers.FirstOrDefaultAsync(x =>
            x.CompanyId == company.Id && x.Code == supplierCode && x.Active);

        if (supplier is null)
            return BadRequest($"Supplier '{supplierCode}' was not found. Register it in suppliers first.");

        var order = new PurchaseOrder
        {
            CompanyId = company.Id,
            CompanyCen = companyCenValue,
            Supplier = supplier.Name,
            SupplierCen = null,
            Status = "DRAFT"
        };

        foreach (var item in request.Items)
        {
            if (!TryParseCen(item.ProductCen, out var productCen)) continue;
            if (item.Quantity != Math.Floor(item.Quantity) || item.Quantity < 1) continue;

            order.Items.Add(new PurchaseOrderItem
            {
                OrderCen = order.Cen,
                ProductCen = productCen,
                Quantity = item.Quantity
            });
        }

        if (order.Items.Count == 0)
            return BadRequest("At least one valid item with a positive whole quantity is required.");

        Db.Orders.Add(order);
        await Db.SaveChangesAsync();
        return Ok(ToDetail(order));
    }

    [HttpGet("{orderCen}")]
    public async Task<IActionResult> GetOrder(string companyCen, string orderCen)
    {
        var order = await FindOrderAsync(companyCen, orderCen);
        return order is null ? NotFound() : Ok(ToDetail(order));
    }

    [HttpPost("{orderCen}/confirm")]
    public async Task<IActionResult> Confirm(string companyCen, string orderCen, CancellationToken cancellationToken)
    {
        var order = await FindOrderAsync(companyCen, orderCen);
        if (order is null) return NotFound();
        if (order.Status == "CONFIRMED") return Conflict("Order is already confirmed.");

        foreach (var item in order.Items)
        {
            var increased = await inventoryStockClient.IncreaseStockAsync(companyCen, item.ProductCen.ToString(), item.Quantity, cancellationToken);
            if (!increased) return Conflict($"Inventory could not increase stock for product {item.ProductCen}.");
        }

        order.Status = "CONFIRMED";
        order.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        return Ok(ToDetail(order));
    }

    [HttpPost("{orderCen}/cancel")]
    public async Task<IActionResult> Cancel(string companyCen, string orderCen)
    {
        var order = await FindOrderAsync(companyCen, orderCen);
        if (order is null) return NotFound();
        if (order.Status == "CONFIRMED") return Conflict("Confirmed orders cannot be cancelled.");
        if (order.Status == "CANCELLED") return Conflict("Order is already cancelled.");

        order.Status = "CANCELLED";
        order.UpdatedAt = DateTime.UtcNow;
        await Db.SaveChangesAsync();
        return Ok(ToDetail(order));
    }

    private async Task<PurchaseOrder?> FindOrderAsync(string companyCen, string orderCen)
    {
        if (!TryParseCen(companyCen, out var company) || !TryParseCen(orderCen, out var order)) return null;
        return await Db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.CompanyCen == company && x.Cen == order);
    }

    private static PurchaseOrderDetailDto ToDetail(PurchaseOrder order)
    {
        return new PurchaseOrderDetailDto(
            order.Cen.ToString(),
            order.Supplier,
            order.Status,
            order.Date,
            order.Items.Select(x => new PurchaseOrderItemDto(x.Cen.ToString(), x.ProductCen.ToString(), x.Quantity)).ToList());
    }
}
