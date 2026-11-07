using CraftFlow.Api.Core.Authorization;
using CraftFlow.Api.Core.Behaviors;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.CoreDomainServices.Identity.Users.CreateTenantUser;

public record CreateTenantUserCommand(
    string Email,
    string Password,
    string FullName,
    TenantRole Role = TenantRole.TECHNOLOGIST
) : IRequest<Result<Guid>>, IRequireQuotaValidation, IRequirePermission
{
    public QuotaType QuotaType => QuotaType.USERS_COUNT;
    public AppPermission RequiredPermission => AppPermission.USERS_MANAGE;
}