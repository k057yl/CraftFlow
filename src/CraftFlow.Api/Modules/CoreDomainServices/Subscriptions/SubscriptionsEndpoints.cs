using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ConfirmPayment;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GenerateAccessKey;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetAccessKeys;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RequestInvoice;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RevokeAccessKey;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions;

public static class SubscriptionsEndpoints
{
    private const string ROLE_ADMIN = "Admin";

    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("")
            .WithTags("Subscriptions")
            .RequireAuthorization();

        group.MapPost(SubscriptionsConstants.SUBSCRIPTION_KEYS, async (GenerateAccessKeyCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapGet(SubscriptionsConstants.SUBSCRIPTION_KEYS, async (ISender sender) =>
        {
            var result = await sender.Send(new GetAccessKeysQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapDelete($"{SubscriptionsConstants.SUBSCRIPTION_KEYS}/{{keyId:guid}}", async (Guid keyId, ISender sender) =>
        {
            var result = await sender.Send(new RevokeAccessKeyCommand(keyId));
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        }).RequireAuthorization(new AuthorizeAttribute { Roles = ROLE_ADMIN });

        group.MapGet($"{SubscriptionsConstants.SUBSCRIPTION_KEYS}/my", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAccessKeysQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapPost(SubscriptionsConstants.REQUEST_INVOICE, async (RequestSubscriptionInvoiceCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        //Tests Payment
        group.MapPost(SubscriptionsConstants.SUBSCRIPTION_TEST_CONFIRM, async (ConfirmSubscriptionPaymentCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });
    }
}