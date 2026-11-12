using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.ActivateFree;

public record ActivateFreeModuleCommand(string PlanCode) : IRequest<Result<bool>>;