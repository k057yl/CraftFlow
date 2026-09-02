using CraftFlow.Api.Common.MultiTenancy;
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

        // --- AGING CHAMBERS  ---
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

        group.MapGet("/stock-lots/raw", async (AppDbContext dbContext, ITenantContext tenantContext, CancellationToken cancellationToken) =>
        {
            var lots = await dbContext.StockLots
                .AsNoTracking()
                .Where(s => s.TenantId == tenantContext.TenantId && s.ProductionBatchId == null)
                .Select(s => new
                {
                    s.Id,
                    Name = dbContext.RawMaterials.Where(rm => rm.Id == s.ItemId).Select(rm => rm.Name).FirstOrDefault() != null
                        ? dbContext.RawMaterials.Where(rm => rm.Id == s.ItemId).Select(rm => rm.Name).FirstOrDefault() + " (" + (s.BatchNumber ?? "Б/Н") + ")"
                        : "Сырье (" + (s.BatchNumber ?? "Б/Н") + ")"
                })
                .ToListAsync(cancellationToken);

            return Results.Ok(lots);
        });

        group.MapGet("/stock-lots/products", async (AppDbContext dbContext, ITenantContext tenantContext, CancellationToken cancellationToken) =>
        {
            var productsFromBatches = await (
                from pb in dbContext.ProductionBatches
                join p in dbContext.Products on pb.TargetProductId equals p.Id
                join sl in dbContext.StockLots on pb.Id equals sl.ProductionBatchId into slGroup
                from sl in slGroup.DefaultIfEmpty()
                where pb.TenantId == tenantContext.TenantId
                select new
                {
                    Id = sl != null ? sl.Id : pb.Id,
                    Name = sl != null && !string.IsNullOrEmpty(sl.BatchNumber)
                        ? sl.BatchNumber
                        : (!string.IsNullOrEmpty(pb.Name) ? pb.Name : p.Name + " (Варка #" + pb.Id.ToString().Substring(0, 8).ToUpper() + ")")
                })
                .AsNoTracking()
                .Distinct()
                .ToListAsync(cancellationToken);

            return Results.Ok(productsFromBatches);
        });
    }
}