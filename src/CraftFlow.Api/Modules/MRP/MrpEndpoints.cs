using CraftFlow.Api.Modules.MRP.CalculateRequirements;
using CraftFlow.Api.Modules.MRP.CreateProcurement;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.MRP;

public static class MrpEndpoints
{
    public static void MapMrpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("MRP")
            .RequireAuthorization();

        group.MapGet(MrpConstants.MRP_REQUIREMENTS, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CalculateRequirementsQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost(MrpConstants.MRP_CREATE_PURCHARE_ORDER, async (CreateProcurementFromMrpCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}