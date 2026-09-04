using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.RegisterUser;

public record RegisterUserCommand(
    Guid TenantId,
    string Email,
    string Password,
    string ConfirmPassword,
    string FullName
) : IRequest<Result<Guid>>;