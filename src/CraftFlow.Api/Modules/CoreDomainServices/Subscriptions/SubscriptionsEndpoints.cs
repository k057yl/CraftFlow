using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ApproveInvoice;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ConfirmPayment;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPendingInvoices;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RequestInvoice;
using CraftFlow.SharedKernel.Constants;
using CraftFlow.SharedKernel.Constants.Endpoints;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions;

public static class SubscriptionsEndpoints
{
    public static void MapSubscriptionsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(string.Empty)
            .WithTags("Subscriptions")
            .RequireAuthorization();

        // --- PUBLIC / TENANT USER ENDPOINTS ---
        group.MapPost(SubscriptionsConstants.REQUEST_INVOICE, async (RequestSubscriptionInvoiceCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        group.MapPost(SubscriptionsConstants.SUBSCRIPTION_TEST_CONFIRM, async (ConfirmSubscriptionPaymentCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
        });

        // --- SUPER ADMIN INVOICE MANAGEMENT ---
        group.MapGet(SubscriptionsConstants.PENDING_INVOICES, async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingInvoicesQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });

        group.MapPost("api/subscriptions/invoices/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ApproveInvoiceCommand(id), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : (result.Error.Code == ErrorCodes.Auth.ACCESS_DENIED
                    ? Results.Json(new { Message = result.Error.Code }, statusCode: 403)
                    : Results.BadRequest(result.Error));
        });
    }
}