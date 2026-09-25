using CraftFlow.Api.Modules.MRP.CalculateRequirements;
using CraftFlow.Api.Modules.MRP.CreateProcurement;
using MediatR;

namespace CraftFlow.Api.Modules.MRP;

public static class MrpEndpoints
{
    public static void MapMrpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("MRP")
            .RequireAuthorization();

        group.MapGet(MrpConstants.MRP_REQUIREMENTS, async (ISender sender) =>
        {
            var result = await sender.Send(new CalculateRequirementsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(MrpConstants.MRP_CREATE_PURCHARE_ORDER, async (CreateProcurementFromMrpCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}