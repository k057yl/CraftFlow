using CraftFlow.SharedKernel.Dtos.Subscription;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetMyOverview;

public record GetMySubscriptionOverviewQuery : IRequest<Result<MySubscriptionOverviewDto>>;