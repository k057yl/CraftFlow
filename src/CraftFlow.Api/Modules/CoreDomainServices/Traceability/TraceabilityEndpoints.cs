using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Traceability.GetBackwardTraceability;
using CraftFlow.Api.Modules.CoreDomainServices.Traceability.GetForwardTraceability;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Traceability;

public static class TraceabilityEndpoints
{
    public static void MapTraceabilityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Traceability")
            .RequireAuthorization();

        group.MapGet($"{TraceabilityConstants.TRACEABILITY_FORWARD}/{{stockLotId:guid}}", async (Guid stockLotId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetForwardTraceabilityQuery(stockLotId), ct)).ToHttpResult());

        group.MapGet($"{TraceabilityConstants.TRACEABILITY_BACKWARD}/{{productStockLotId:guid}}", async (Guid productStockLotId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetBackwardTraceabilityQuery(productStockLotId), ct)).ToHttpResult());
    }
}