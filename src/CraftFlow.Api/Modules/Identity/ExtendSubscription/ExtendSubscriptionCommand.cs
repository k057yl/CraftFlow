using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.ExtendSubscription;
public record ExtendSubscriptionCommand(Guid OrganizationId, int Days) : IRequest<Result<bool>>;