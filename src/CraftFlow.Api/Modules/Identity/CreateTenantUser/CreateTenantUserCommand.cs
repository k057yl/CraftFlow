using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;
public record CreateTenantUserCommand(
    string Email,
    string Password,
    string FullName
) : IRequest<Result<Guid>>;
