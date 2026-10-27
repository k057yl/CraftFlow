using CraftFlow.Api.Modules.Traceability.GetBackwardTraceability;
using CraftFlow.Api.Modules.Traceability.GetForwardTraceability;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Traceability;

public static class TraceabilityEndpoints
{
    public static void MapTraceabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Traceability")
            .RequireAuthorization();

        group.MapGet($"{TraceabilityConstants.TRACEABILITY_FORWARD}/{{stockLotId:guid}}", async (Guid stockLotId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetForwardTraceabilityQuery(stockLotId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : (result.Error.Code == ErrorCodes.Inventory.ITEM_NOT_FOUND
                        ? Results.NotFound(result.Error)
                        : Results.BadRequest(result.Error)));
        });

        group.MapGet($"{TraceabilityConstants.TRACEABILITY_BACKWARD}/{{productStockLotId:guid}}", async (Guid productStockLotId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBackwardTraceabilityQuery(productStockLotId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : (result.Error.Code == ErrorCodes.Inventory.ITEM_NOT_FOUND
                        ? Results.NotFound(result.Error)
                        : Results.BadRequest(result.Error)));
        });
    }
}