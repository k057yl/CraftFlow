using CraftFlow.Api.Modules.Sales.Customers.CreateCustomer;
using CraftFlow.Api.Modules.Sales.Customers.GetCustomers;
using CraftFlow.Api.Modules.Sales.Orders.CreateAndShipSalesOrder;
using CraftFlow.Api.Modules.Sales.Stock.GetSalesStockInfo;
using CraftFlow.Api.Modules.Sales.Stock.GetStockLotDetails;
using MediatR;

namespace CraftFlow.Api.Modules.Sales;

public static class SalesEndpoints
{
    public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Sales")
            .RequireAuthorization();

        group.MapPost(SalesConstants.CUSTOMERS, async (CreateCustomerCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(SalesConstants.CUSTOMERS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetCustomersQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(SalesConstants.ORDERS_SHIP, async (CreateAndShipSalesOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(SalesConstants.SALES_STOCK_INFO, async (Guid warehouseId, Guid productId, ISender sender) =>
        {
            var result = await sender.Send(new GetSalesStockInfoQuery(warehouseId, productId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet(SalesConstants.SALES_STOCK_DETAIL_INFO, async (Guid lotId, ISender sender) =>
        {
            var result = await sender.Send(new GetStockLotDetailsQuery(lotId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}