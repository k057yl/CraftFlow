using CraftFlow.Api.Modules.MRP.CalculateRequirements;
using MediatR;

namespace CraftFlow.Api.Modules.MRP;

public static class MrpEndpoints
{
    public static void MapMrpEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/mrp");

        group.MapGet("requirements", async (ISender sender) =>
        {
            var result = await sender.Send(new CalculateRequirementsQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}