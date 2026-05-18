using Purchases.Api.Application.Abstractions;
using Purchases.Api.Infrastructure.Inventory;
using Purchases.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore; // 👈 Agregar

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_CONNECTION")
    ?? throw new InvalidOperationException("Missing DefaultConnection or DATABASE_CONNECTION.");

var inventoryBaseUrl = builder.Configuration["Services:InventoryApi"]
    ?? Environment.GetEnvironmentVariable("INVENTORY_API_URL")
    ?? "http://localhost:5143";

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PurchasesDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
        npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "purchases")));
builder.Services.AddHttpClient<IInventoryStockClient, InventoryStockClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
            ?? ["http://localhost:5173"];

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PurchasesDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Purchases.Api");
        logger.LogWarning(ex, "Could not apply purchases database migrations.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // 👈 Agregar
}

app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();