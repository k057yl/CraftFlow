using CraftFlow.Api.Core.Authorization;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.GetSubscriptionPayments;

public record GetSubscriptionPaymentsQuery(Guid OrganizationId) : IRequest<Result<List<PaymentDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SUBSCRIPTION_MANAGE;
}