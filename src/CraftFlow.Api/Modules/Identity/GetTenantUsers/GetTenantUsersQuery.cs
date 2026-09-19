using CraftFlow.SharedKernel.Dtos.Auth;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.GetTenantUsers;

public sealed record GetTenantUsersQuery : IRequest<Result<List<TenantUserDto>>>;