using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.AddStockLot;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetProductStockLots;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetRawStockLots;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.GetStockLots;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.ReceiveStockBatch;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StockLots.WriteOffStockLot;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.CreateStorageLocation;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.DeleteStorageLocation;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.GetStorageLocations;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.StorageLocations.WriteOffStorageLocation;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.CreateWarehouse;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.DeleteWarehouse;
using CraftFlow.Api.Modules.CoreDomainServices.Inventory.Warehouses.GetWarehouses;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Inventory")
            .RequireAuthorization();

        // --- WAREHOUSES ---
        group.MapPost(InventoryConstants.WAREHOUSES, async (CreateWarehouseCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(InventoryConstants.WAREHOUSES, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetWarehousesQuery(), ct)).ToHttpResult());

        group.MapDelete($"{InventoryConstants.WAREHOUSES}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteWarehouseCommand(id), ct)).ToHttpResult());

        // --- STORAGE LOCATIONS ---
        group.MapPost(InventoryConstants.WAREHOUSES_LOCATIONS, async (CreateStorageLocationCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(InventoryConstants.WAREHOUSES_LOCATIONS, async (Guid? warehouseId, Guid? chamberId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetStorageLocationsQuery(warehouseId, chamberId), ct)).ToHttpResult());

        group.MapDelete($"{InventoryConstants.WAREHOUSES_LOCATIONS}/{{id:guid}}", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new DeleteStorageLocationCommand(id), ct)).ToHttpResult());

        group.MapPost(InventoryConstants.WAREHOUSES_LOCATIONS_WRITE_OFF, async (WriteOffStorageLocationCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        // --- STOCK LOTS ---
        group.MapPost(InventoryConstants.STOCK_LOTS, async (AddStockLotCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(InventoryConstants.STOCK_LOTS, async (
            Guid? warehouseId,
            Guid? supplierId,
            bool? onlyExpiringSoon,
            bool? onlyExpired,
            string? sortBy,
            ISender sender,
            CancellationToken ct) =>
            (await sender.Send(new GetStockLotsQuery(warehouseId, supplierId, onlyExpiringSoon, onlyExpired, sortBy), ct)).ToHttpResult());

        group.MapGet(InventoryConstants.STOCK_LOTS_RAW, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetRawStockLotsQuery(), ct)).ToHttpResult());

        group.MapGet(InventoryConstants.STOCK_LOTS_PRODUCTS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetProductStockLotsQuery(), ct)).ToHttpResult());

        group.MapPost(InventoryConstants.STOCK_LOTS_BATCH, async (ReceiveStockBatchCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(InventoryConstants.STOCK_LOTS_WRITE_OFF, async (WriteOffStockLotCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());
    }
}