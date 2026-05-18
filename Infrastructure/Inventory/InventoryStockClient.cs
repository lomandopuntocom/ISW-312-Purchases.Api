using System.Net.Http.Json;
using Purchases.Api.Application.Abstractions;

namespace Purchases.Api.Infrastructure.Inventory;

public sealed class InventoryStockClient(HttpClient httpClient) : IInventoryStockClient
{
    public async Task<bool> IncreaseStockAsync(string companyCen, string productCen, decimal quantity, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync(
            $"/api/inventory/companies/{companyCen}/stock/increase",
            new { productCen, quantity, reference = "PURCHASE", notes = "Purchase order confirmation" },
            cancellationToken);

        return response.IsSuccessStatusCode;
    }
}
