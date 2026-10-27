using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Users.GetTenantUsers;

public sealed record GetTenantUsersQuery : IRequest<Result<List<TenantUserDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.USERS_MANAGE;
}