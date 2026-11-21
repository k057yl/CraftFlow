using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Customers.CreateCustomer;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Customers.GetCustomers;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Orders.CreateAndShipSalesOrder;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Stock.GetSalesStockInfo;
using CraftFlow.Api.Modules.CoreDomainServices.Sales.Stock.GetStockLotDetails;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Sales;

public static class SalesEndpoints
{
    public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Sales")
            .RequireAuthorization();

        group.MapPost(SalesConstants.CUSTOMERS, async (CreateCustomerCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(SalesConstants.CUSTOMERS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetCustomersQuery(), ct)).ToHttpResult());

        group.MapPost(SalesConstants.ORDERS_SHIP, async (CreateAndShipSalesOrderCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(SalesConstants.SALES_STOCK_INFO, async (Guid warehouseId, Guid productId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSalesStockInfoQuery(warehouseId, productId), ct)).ToHttpResult());

        group.MapGet(SalesConstants.SALES_STOCK_DETAIL_INFO, async (Guid lotId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetStockLotDetailsQuery(lotId), ct)).ToHttpResult());
    }
}