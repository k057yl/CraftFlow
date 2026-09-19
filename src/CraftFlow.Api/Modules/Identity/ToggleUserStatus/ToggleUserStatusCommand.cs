using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.ToggleUserStatus;

public sealed record ToggleUserStatusCommand(Guid UserId) : IRequest<Result<bool>>;