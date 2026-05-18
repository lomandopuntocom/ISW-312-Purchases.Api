namespace Purchases.Api.Application.Abstractions;

public interface IInventoryStockClient
{
    Task<bool> IncreaseStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken);
}
