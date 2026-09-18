using CraftFlow.Api.Modules.Identity.Domain;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.CreateTenantUser;
public record CreateTenantUserCommand(
    string Email,
    string Password,
    string FullName,
    TenantRole Role = TenantRole.Technologist
) : IRequest<Result<Guid>>;
