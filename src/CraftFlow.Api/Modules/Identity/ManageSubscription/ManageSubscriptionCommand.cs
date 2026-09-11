using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.ManageSubscription;
public record ManageSubscriptionCommand(
    Guid OrganizationId,
    string PlanCode,
    int AddDays,
    bool IsActive
) : IRequest<Result<bool>>;