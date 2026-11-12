using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetPlans;

public record GetSubscriptionPlansQuery(Guid? OrganizationId = null) : IRequest<Result<List<SubscriptionPlanDto>>>;