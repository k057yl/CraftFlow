using CraftFlow.Api.Modules.Subscriptions.GenerateAccessKey;
using CraftFlow.SharedKernel.Constants;
using MediatR;

namespace CraftFlow.Api.Modules.Subscriptions;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Subscriptions")
            .RequireAuthorization();

        group.MapPost(Endpoints.SUBSCRIPTION_KEYS, async (GenerateAccessKeyCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}