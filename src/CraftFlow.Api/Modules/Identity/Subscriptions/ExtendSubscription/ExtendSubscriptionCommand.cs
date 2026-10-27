using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Subscriptions.ExtendSubscription;

public record ExtendSubscriptionCommand(Guid OrganizationId, int Days) : IRequest<Result<bool>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SUBSCRIPTION_MANAGE;
}