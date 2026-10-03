using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Subscriptions.ConfirmPayment;

public record ConfirmSubscriptionPaymentCommand(string PlanCode, int Days = 30) : IRequest<Result>;
