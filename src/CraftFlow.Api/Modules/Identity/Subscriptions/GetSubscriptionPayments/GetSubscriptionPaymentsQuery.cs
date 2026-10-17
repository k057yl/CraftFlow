using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Subscriptions.GetSubscriptionPayments;
public record GetSubscriptionPaymentsQuery(Guid OrganizationId) : IRequest<Result<List<PaymentDto>>>;