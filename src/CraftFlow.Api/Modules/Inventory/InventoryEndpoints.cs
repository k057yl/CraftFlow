using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Aging.CreateChamber;
using CraftFlow.Api.Modules.Inventory.AddStockLot;
using CraftFlow.Api.Modules.Inventory.CreateWarehouse;
using CraftFlow.Api.Modules.Inventory.GetWarehouses;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Inventory")
            .RequireAuthorization();

        // --- WAREHOUSES ---
        group.MapPost(Endpoints.WAREHOUSES, async (CreateWarehouseCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.WAREHOUSES, async (ISender sender) =>
        {
            var result = await sender.Send(new GetWarehousesQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // --- AGING CHAMBERS ---
        group.MapPost(Endpoints.AGING_CHAMBERS, async (CreateChamberCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.AGING_CHAMBERS, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var chambers = await dbContext.AgingChambers
                .AsNoTracking()
                .Select(c => new { c.Id, Name = c.Name })
                .ToListAsync(cancellationToken);

            return Results.Ok(chambers);
        });

        // --- STOCK LOTS ---
        group.MapPost(Endpoints.STOCK_LOTS, async (AddStockLotCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.STOCK_LOTS, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var lots = await dbContext.StockLots
                .AsNoTracking()
                .Select(l => new { l.Id, Name = l.BatchNumber })
                .ToListAsync(cancellationToken);

            return Results.Ok(lots);
        });

        // --- СЫРЬЕВЫЕ ЛОТЫ ДЛЯ ПРЯМОЙ ТРАССИРОВКИ ---
        group.MapGet(Endpoints.STOCK_LOTS_RAW, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var rawLots = await dbContext.StockLots
                .AsNoTracking()
                .Join(dbContext.RawMaterials,
                    sl => sl.ItemId,
                    rm => rm.Id,
                    (sl, rm) => new
                    {
                        sl.Id,
                        Name = rm.Name + " (" + (sl.BatchNumber ?? "NoN") + " | Remainder: " + sl.Quantity + ")"
                    })
                .ToListAsync(cancellationToken);

            return Results.Ok(rawLots);
        });

        // --- ЛОТЫ ГОТОВОЙ ПРОДУКЦИИ ДЛЯ ОБРАТНОЙ ТРАССИРОВКИ ---
        group.MapGet(Endpoints.STOCK_LOTS_PRODUCTS, async (AppDbContext dbContext, CancellationToken cancellationToken) =>
        {
            var productLots = await dbContext.StockLots
                .AsNoTracking()
                .Join(dbContext.Products,
                    sl => sl.ItemId,
                    p => p.Id,
                    (sl, p) => new
                    {
                        sl.Id,
                        Name = p.Name + " (" + (sl.BatchNumber ?? "NoN") + " | In stock: " + sl.Quantity + " kg)"
                    })
                .ToListAsync(cancellationToken);

            return Results.Ok(productLots);
        });
    }
}