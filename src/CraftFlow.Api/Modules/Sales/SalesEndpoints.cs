using CraftFlow.Api.Common.Persistence;
using CraftFlow.Api.Modules.Sales.CreateAndShipSalesOrder;
using CraftFlow.Api.Modules.Sales.CreateCustomer;
using CraftFlow.Api.Modules.Sales.GetCustomers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CraftFlow.Api.Modules.Sales
{
    public static class SalesEndpoints
    {
        public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("api/sales")
                .WithTags("Sales");

            group.MapPost("/customers", async (CreateCustomerCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/customers", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCustomersQuery());
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapPost("/orders/ship", async (CreateAndShipSalesOrderCommand command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            });

            group.MapGet("/stock-info", async (Guid warehouseId, Guid productId, AppDbContext dbContext) =>
            {
                var activeLots = await dbContext.StockLots
                    .Where(s => s.WarehouseId == warehouseId && s.ItemId == productId && s.Quantity > 0)
                    .ToListAsync();

                var totalQuantity = activeLots.Sum(s => s.Quantity);

                decimal unitCost = 0m;
                if (totalQuantity > 0)
                {
                    unitCost = activeLots.Sum(l => l.Quantity * l.UnitPrice) / totalQuantity;
                }

                decimal basePrice = unitCost > 0 ? unitCost * 1.40m : 450.00m;

                return Results.Ok(new
                {
                    Quantity = totalQuantity,
                    UnitCost = unitCost,
                    BasePrice = basePrice
                });
            });
        }
    }
}