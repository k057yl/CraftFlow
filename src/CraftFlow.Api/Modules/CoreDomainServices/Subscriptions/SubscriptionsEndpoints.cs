using CraftFlow.Api.Core.Extensions;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ApproveInvoice;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ConfirmPayment;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetMyOverview;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPendingInvoices;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPlans;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RejectInvoice;
using CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RequestInvoice;
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

        group.MapPost(SubscriptionsConstants.REQUEST_INVOICE, async (RequestSubscriptionInvoiceCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapPost(SubscriptionsConstants.SUBSCRIPTION_TEST_CONFIRM, async (ConfirmSubscriptionPaymentCommand command, ISender sender, CancellationToken ct) =>
            (await sender.Send(command, ct)).ToHttpResult());

        group.MapGet(SubscriptionsConstants.PENDING_INVOICES, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetPendingInvoicesQuery(), ct)).ToHttpResult());

        group.MapPost("api/subscriptions/invoices/{id:guid}/approve", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new ApproveInvoiceCommand(id), ct)).ToHttpResult());

        group.MapGet(SubscriptionsConstants.MY_SUBSCRIPTIONS, async (ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetMySubscriptionOverviewQuery(), ct)).ToHttpResult());

        group.MapGet(SubscriptionsConstants.PLANS, async (Guid? organizationId, ISender sender, CancellationToken ct) =>
            (await sender.Send(new GetSubscriptionPlansQuery(organizationId), ct)).ToHttpResult());

        group.MapPost("api/subscriptions/invoices/{id:guid}/reject", async (Guid id, ISender sender, CancellationToken ct) =>
            (await sender.Send(new RejectInvoiceCommand(id), ct)).ToHttpResult());
    }
}