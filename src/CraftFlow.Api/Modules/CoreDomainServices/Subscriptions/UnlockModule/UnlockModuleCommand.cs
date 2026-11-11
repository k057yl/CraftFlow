using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Subscriptions.UnlockModule;

public record UnlockModuleCommand(string FreePlanCode) : IRequest<Result<bool>>;