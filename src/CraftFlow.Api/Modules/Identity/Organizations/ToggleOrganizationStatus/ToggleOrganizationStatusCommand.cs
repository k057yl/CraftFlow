using CraftFlow.Api.Common.Authorization;
using CraftFlow.SharedKernel.Enums.Identity;
using CraftFlow.SharedKernel.Result;
using MediatR;

namespace CraftFlow.Api.Modules.Identity.Organizations.ToggleOrganizationStatus;

public record ToggleOrganizationStatusCommand(Guid OrganizationId) : IRequest<Result<bool>>, IRequirePermission
{
    public AppPermission RequiredPermission => AppPermission.SYSTEM_ADMIN;
}