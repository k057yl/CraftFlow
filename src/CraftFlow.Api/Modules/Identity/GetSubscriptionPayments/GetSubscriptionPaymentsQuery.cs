using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.GetSubscriptionPayments;
public record GetSubscriptionPaymentsQuery(Guid OrganizationId) : IRequest<Result<List<PaymentDto>>>;