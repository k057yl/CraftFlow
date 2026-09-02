using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.CreateChamber;
using CraftFlow.Api.Modules.Inventory.AddStockLot;
using CraftFlow.Api.Modules.Inventory.CreateWarehouse;
using CraftFlow.Api.Modules.Inventory.GetWarehouses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/inventory")
            .WithTags("Inventory");

        // --- WAREHOUSES ---
        group.MapPost("/warehouses", async (CreateWarehouseCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/warehouses", async (ISender sender) =>
        {
            var result = await sender.Send(new GetWarehousesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // --- AGING CHAMBERS ---
        group.MapPost("/aging-chambers", async (CreateChamberCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/aging-chambers", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var chambers = await dbContext.AgingChambers
                .AsNoTracking()
                .Select(c => new { c.Id, Name = c.Name })
                .ToListAsync(cancellationToken);

            return Results.Ok(chambers);
        });

        // --- STOCK LOTS ---
        group.MapPost("/stock-lots", async (AddStockLotCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/stock-lots", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var lots = await dbContext.StockLots
                .AsNoTracking()
                .Select(l => new { l.Id, Name = l.BatchNumber })
                .ToListAsync(cancellationToken);

            return Results.Ok(lots);
        });

        // --- СЫРЬЕВЫЕ ЛОТЫ ДЛЯ ПРЯМОЙ ТРАССИРОВКИ ---
        group.MapGet("/stock-lots/raw", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var rawLots = await dbContext.StockLots
                .AsNoTracking()
                .Join(dbContext.RawMaterials,
                    sl => sl.ItemId,
                    rm => rm.Id,
                    (sl, rm) => new
                    {
                        sl.Id,
                        Name = rm.Name + " (" + (sl.BatchNumber ?? "Б/Н") + " | Остаток: " + sl.Quantity + ")"
                    })
                .ToListAsync(cancellationToken);

            return Results.Ok(rawLots);
        });

        // --- ЛОТЫ ГОТОВОЙ ПРОДУКЦИИ ДЛЯ ОБРАТНОЙ ТРАССИРОВКИ ---
        group.MapGet("/stock-lots/products", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var productLots = await dbContext.StockLots
                .AsNoTracking()
                .Join(dbContext.Products,
                    sl => sl.ItemId,
                    p => p.Id,
                    (sl, p) => new
                    {
                        sl.Id,
                        Name = p.Name + " (" + (sl.BatchNumber ?? "Б/Н") + " | На складе: " + sl.Quantity + " кг)"
                    })
                .ToListAsync(cancellationToken);

            return Results.Ok(productLots);
        });
    }
}