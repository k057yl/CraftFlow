using CraftFlow.Api.Modules.Subscriptions.GenerateAccessKey;
using CraftFlow.Api.Modules.Subscriptions.GetAccessKeys;
using CraftFlow.Api.Modules.Subscriptions.RevokeAccessKey;
using CraftFlow.SharedKernel.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CraftFlow.Api.Modules.Subscriptions;

public static class SubscriptionsEndpoints
{
    private const string ROLE_ADMIN = "Admin";

    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Subscriptions")
            .RequireAuthorization();

        group.MapPost(Endpoints.SUBSCRIPTION_KEYS, async (GenerateAccessKeyCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapGet(Endpoints.SUBSCRIPTION_KEYS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetAccessKeysQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapDelete($"{Endpoints.SUBSCRIPTION_KEYS}/{{keyId:guid}}", async (Guid keyId, ISender sender) =>
        {
            var result = await sender.Send(new RevokeAccessKeyCommand(keyId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapGet($"{Endpoints.SUBSCRIPTION_KEYS}/my", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAccessKeysQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}