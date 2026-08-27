using CraftFlow.Api.Modules.Inventory.AddStockLot;
using CraftFlow.Api.Modules.Inventory.CreateWarehouse;
using CraftFlow.Api.Modules.Inventory.GetWarehouses;
using MediatR;

namespace CraftFlow.Api.Modules.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/inventory")
            .WithTags("Inventory");

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

        group.MapPost("/stock-lots", async (AddStockLotCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}