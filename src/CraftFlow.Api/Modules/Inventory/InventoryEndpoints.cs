using CraftFlow.Api.Modules.Inventory.StockLots.AddStockLot;
using CraftFlow.Api.Modules.Inventory.StockLots.GetProductStockLots;
using CraftFlow.Api.Modules.Inventory.StockLots.GetRawStockLots;
using CraftFlow.Api.Modules.Inventory.StockLots.GetStockLots;
using CraftFlow.Api.Modules.Inventory.StockLots.WriteOffStockLot;
using CraftFlow.Api.Modules.Inventory.StorageLocations.CreateStorageLocation;
using CraftFlow.Api.Modules.Inventory.StorageLocations.DeleteStorageLocation;
using CraftFlow.Api.Modules.Inventory.StorageLocations.GetStorageLocations;
using CraftFlow.Api.Modules.Inventory.Warehouses.CreateWarehouse;
using CraftFlow.Api.Modules.Inventory.Warehouses.DeleteWarehouse;
using CraftFlow.Api.Modules.Inventory.Warehouses.GetWarehouses;
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
        group.MapPost(InventoryConstants.WAREHOUSES, async (CreateWarehouseCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapGet(InventoryConstants.WAREHOUSES, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetWarehousesQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{InventoryConstants.WAREHOUSES}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteWarehouseCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- STORAGE LOCATIONS ---
        group.MapPost($"{InventoryConstants.WAREHOUSES}/locations", async (CreateStorageLocationCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapGet($"{InventoryConstants.WAREHOUSES}/locations", async (Guid? warehouseId, Guid? chamberId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStorageLocationsQuery(warehouseId, chamberId), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapDelete($"{InventoryConstants.WAREHOUSES}/locations/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteStorageLocationCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok()
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        // --- STOCK LOTS ---
        group.MapPost(InventoryConstants.STOCK_LOTS, async (AddStockLotCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapGet(InventoryConstants.STOCK_LOTS, async (
            Guid? warehouseId,
            Guid? supplierId,
            bool? onlyExpiringSoon,
            bool? onlyExpired,
            string? sortBy,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStockLotsQuery(warehouseId, supplierId, onlyExpiringSoon, onlyExpired, sortBy), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(InventoryConstants.STOCK_LOTS_RAW, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRawStockLotsQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(InventoryConstants.STOCK_LOTS_PRODUCTS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductStockLotsQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost($"{InventoryConstants.STOCK_LOTS}/write-off", async (WriteOffStockLotCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}