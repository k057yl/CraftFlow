using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Users.CreateTenantUser;

public record CreateTenantUserCommand(
    string Email,
    string Password,
    string FullName,
    TenantRole Role = TenantRole.TECHNOLOGIST
) : IRequest<Result<Guid>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.USERS_MANAGE;
}