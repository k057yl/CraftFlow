using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ConfirmPayment;

public record ConfirmSubscriptionPaymentCommand(
    string PlanCode,
    Guid? TargetTenantId = null,
    int Days = 30) : IRequest<Result>;
