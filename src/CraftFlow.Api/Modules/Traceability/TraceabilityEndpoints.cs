using CraftFlow.Api.Modules.Traceability.GetBackwardTraceability;
using CraftFlow.Api.Modules.Traceability.GetForwardTraceability;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability;

public static class TraceabilityEndpoints
{
    public static void MapTraceabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/traceability");

        group.MapGet("forward/{stockLotId:guid}", async (Guid stockLotId, ISender sender) =>
        {
            var result = await sender.Send(new GetForwardTraceabilityQuery(stockLotId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("backward/{productStockLotId:guid}", async (Guid productStockLotId, ISender sender) =>
        {
            var result = await sender.Send(new GetBackwardTraceabilityQuery(productStockLotId));
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}