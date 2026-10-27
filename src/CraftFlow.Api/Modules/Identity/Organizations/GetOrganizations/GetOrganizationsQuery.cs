using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Dtos.Identity;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Organizations.GetOrganizations;

public record GetOrganizationsQuery : IRequest<Result<List<OrganizationAdminDto>>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SYSTEM_ADMIN;
}