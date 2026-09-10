using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.DeleteAccount;

public record DeleteAccountCommand(Guid UserId) : IRequest<Result<bool>>;