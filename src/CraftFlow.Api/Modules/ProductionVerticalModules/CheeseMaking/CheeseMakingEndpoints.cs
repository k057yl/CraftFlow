using CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking.Production.Batches.GetBatchesReadyForAging;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.ProductionVerticalModules.CheeseMaking;

public static class CheeseMakingEndpoints
{
    public static void MapCheeseMakingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Cheese Making")
            .RequireAuthorization();

        group.MapGet(ProductionConstants.BATCHES_READY_AGING, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBatchesReadyForAgingQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}