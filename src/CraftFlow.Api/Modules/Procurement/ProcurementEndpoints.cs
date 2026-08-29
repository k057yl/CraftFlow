using CraftFlow.Api.Modules.Procurement.PurchaseOrders;
using CraftFlow.Api.Modules.Procurement.Suppliers;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Procurement;

public static class ProcurementEndpoints
{
    public static void MapProcurementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(Endpoints.SUPPLIERS, async (CreateSupplierCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        app.MapPost(Endpoints.PURCHASE_ORDERS_RECEIVE, async (ReceiveGoodsCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}