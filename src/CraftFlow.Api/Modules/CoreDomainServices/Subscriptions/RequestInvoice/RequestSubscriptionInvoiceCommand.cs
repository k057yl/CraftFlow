using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.RequestInvoice;

public record RequestSubscriptionInvoiceCommand(string PlanCode, int Days = 30) : IRequest<Result>;