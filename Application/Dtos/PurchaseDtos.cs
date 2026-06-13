namespace Purchases.Api.Application.Dtos;

public sealed record PagedResultDto<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
public sealed record PurchaseOrderListDto(string Cen, string Supplier, string Status, DateTime Date, int ItemCount);
public sealed record PurchaseOrderDetailDto(string Cen, string Supplier, string Status, DateTime Date, IReadOnlyList<PurchaseOrderItemDto> Items);
public sealed record PurchaseOrderItemDto(string Cen, string ProductCen, decimal Quantity);
public sealed record SupplierDto(string SupplierCen, string Code, string Name, bool Active);
public sealed record UpsertSupplierRequest(string Name, string? Code);
public sealed record CreatePurchaseOrderRequest(string? Supplier, string? SupplierCen, IReadOnlyList<CreatePurchaseOrderItemRequest> Items);
public sealed record CreatePurchaseOrderItemRequest(string ProductCen, decimal Quantity);

public sealed record UpdateSupplierContractRequest(string Name);
public sealed record SupplierContractDto(string SupplierCen, string Name);
public sealed record PurchaseOrderLineContractDto(string ProductCen, double Quantity);
public sealed record CreatePurchaseOrderContractRequest(string SupplierCen, string? WarehouseCen, IReadOnlyList<PurchaseOrderLineContractDto> Items);
public sealed record PurchaseOrderContractDto(
    string OrderCen,
    string SupplierCen,
    string? WarehouseCen,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseOrderLineContractDto> Items
);

