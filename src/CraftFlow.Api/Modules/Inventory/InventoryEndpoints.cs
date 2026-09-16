using CraftFlow.Api.Modules.Inventory.AddStockLot;
using CraftFlow.Api.Modules.Inventory.CreateWarehouse;
using CraftFlow.Api.Modules.Inventory.DeleteWarehouse;
using CraftFlow.Api.Modules.Inventory.GetProductStockLots;
using CraftFlow.Api.Modules.Inventory.GetRawStockLots;
using CraftFlow.Api.Modules.Inventory.GetStockLots;
using CraftFlow.Api.Modules.Inventory.GetWarehouses;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
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

        group.MapDelete($"{Endpoints.WAREHOUSES}/{{id:guid}}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteWarehouseCommand(id));
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- STOCK LOTS ---
        group.MapPost(Endpoints.STOCK_LOTS, async (AddStockLotCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(Endpoints.STOCK_LOTS, async (
            Guid? warehouseId,
            Guid? supplierId,
            bool? onlyExpiringSoon,
            bool? onlyExpired,
            string? sortBy,
            ISender sender) =>
        {
            var result = await sender.Send(new GetStockLotsQuery(warehouseId, supplierId, onlyExpiringSoon, onlyExpired, sortBy));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // --- СЫРЬЕВЫЕ ЛОТЫ ДЛЯ ПРЯМОЙ ТРАССИРОВКИ ---
        group.MapGet(Endpoints.STOCK_LOTS_RAW, async (ISender sender) =>
        {
            var result = await sender.Send(new GetRawStockLotsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        // --- ЛОТЫ ГОТОВОЙ ПРОДУКЦИИ ДЛЯ ОБРАТНОЙ ТРАССИРОВКИ ---
        group.MapGet(Endpoints.STOCK_LOTS_PRODUCTS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetProductStockLotsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}