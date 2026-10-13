using CraftFlow.Api.Modules.Sanitation.CreateSanitationRule;
using CraftFlow.Api.Modules.Sanitation.GetSanitationDetails;
using CraftFlow.SharedKernel.Constants.Endpoints;
using CraftFlow.SharedKernel.Enums.Sanitation;
using MediatR;

namespace CraftFlow.Api.Modules.Sanitation;

public static class SanitationEndpoints
{
    public static void MapSanitationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Sanitation")
            .RequireAuthorization();

        group.MapPost(SanitationConstants.RULES, async (CreateSanitationRuleCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        });

        group.MapGet(SanitationConstants.WAREHOUSE_DETAILS, async (SanitationTargetType targetType, Guid targetId, ISender sender) =>
        {
            var result = await sender.Send(new GetSanitationDetailsQuery(targetType, targetId));
            return Results.Ok(result);
        });
    }
}